// サーバーとやり取りするデータ構造と、画面上で編集するためのデータ構造との相互変換。
// サーバー側はすべてのノードを深さ付きのフラットな配列で扱うが、
// 画面側では編集単位ごとに配列を分けておいた方がグリッド等にそのままバインドできて扱いやすいため、ここで組み替える。

import {
  NODE_TYPE_STATIC_ENUM,
  NODE_TYPE_DYNAMIC_ENUM_TYPE,
  NODE_TYPE_VALUE_OBJECT,
  type ClientRequest,
  type Config,
  type GraphLayout,
  type InitialLoadData,
  type OptionalAttributeDef,
  type OptionalAttributeValue,
  type SchemaNode,
  type SchemaNodeTypeDef,
} from "./types"

/** 画面上で編集するプロジェクトのデータ */
export type EditingProject = {
  projectRoot?: string | null
  editingXmlFilePath?: string | null
  config: Config
  /** 区分定義・値オブジェクト以外のルート要素とその子孫 */
  rootAggregates: RootAggregateDef[]
  /** 静的区分（列挙体）の定義 */
  staticEnums: StaticEnumDef[]
  /** 動的区分（区分マスタ）の種類。いずれも子要素を持たないルート要素 */
  dynamicEnumTypes: EditingSchemaNode[]
  /** 値オブジェクトの定義。いずれも子要素を持たないルート要素 */
  valueObjects: EditingSchemaNode[]
  aggregateOrMemberTypes: SchemaNodeTypeDef[]
  optionalAttributes: OptionalAttributeDef[]
  /** ダイアグラム上で位置を動かしたルート集約のノードの位置 */
  graphLayout: GraphLayout
}

/**
 * 画面上で編集する集約または集約メンバー。
 * オプショナル属性を属性のキーで引けるようにしたこと以外はサーバー側の構造と同じ。
 */
export type EditingSchemaNode = Omit<SchemaNode, "attrValues"> & {
  /**
   * オプショナル属性の値。キーは属性のキー。
   * 真偽値型の属性は true または false、それ以外の属性は文字列。
   * キーが存在しない場合、または値が false か空文字の場合は指定なしを表す。
   */
  attrs: { [key: string]: string | boolean }
}

/** 区分定義以外のルート要素1個分の定義 */
export type RootAggregateDef = {
  /** ルート要素 */
  root: EditingSchemaNode
  /**
   * ルート要素の子孫。サーバー側と同じく深さ付きのフラットな配列。
   * 親子関係は並び順と深さから導出される。直前の行より深い行はその行の子になる。
   * 深さは1以上で、直前の行の深さ + 1 を超えてはならない。
   */
  members: EditingSchemaNode[]
}

/** 静的区分（列挙体）1種類分の定義 */
export type StaticEnumDef = {
  /** 区分の種類を表すルート要素 */
  root: EditingSchemaNode
  /**
   * 区分の値。
   * displayName が区分名、typeDetail が区分値を表す。
   */
  values: EditingSchemaNode[]
}

/**
 * サーバーから受け取ったデータを画面上で編集するためのデータ構造に変換する。
 */
export function toEditingProject(data: InitialLoadData): EditingProject {
  const optionalAttributes = data.optionalAttributes ?? []
  const attrDefs = new Map(optionalAttributes.map(def => [def.key, def]))

  const rootAggregates: RootAggregateDef[] = []
  const staticEnums: StaticEnumDef[] = []
  const dynamicEnumTypes: EditingSchemaNode[] = []
  const valueObjects: EditingSchemaNode[] = []

  // ルート要素ごとに、その種類に応じた振り分け先へ子孫ごと移す
  const nodes = (data.aggregates ?? []).map(node => toEditingSchemaNode(node, attrDefs))
  for (const [root, ...descendants] of splitByRoot(nodes)) {
    if (root.type === NODE_TYPE_STATIC_ENUM) {
      staticEnums.push({ root, values: descendants })
    } else if (root.type === NODE_TYPE_DYNAMIC_ENUM_TYPE) {
      dynamicEnumTypes.push(root)
    } else if (root.type === NODE_TYPE_VALUE_OBJECT) {
      valueObjects.push(root)
    } else {
      rootAggregates.push({ root, members: descendants })
    }
  }

  return {
    projectRoot: data.projectRoot,
    editingXmlFilePath: data.editingXmlFilePath,
    config: data.config ?? createDefaultConfig(),
    rootAggregates,
    staticEnums,
    dynamicEnumTypes,
    valueObjects,
    aggregateOrMemberTypes: data.aggregateOrMemberTypes ?? [],
    optionalAttributes,
    graphLayout: data.graphLayout ?? {},
  }
}

/**
 * 画面上で編集したデータをサーバーに送るデータ構造に変換する。
 * ルート要素の並び順は、集約、静的区分、動的区分の種類、値オブジェクトの順になる。
 * 画面上の編集操作によって深さの連続性が崩れている場合、ここで補正する。
 */
export function toClientRequest(project: EditingProject): ClientRequest {
  return {
    config: project.config,
    aggregates: [
      ...project.rootAggregates.flatMap(({ root, members }) => [
        toSchemaNode({ ...root, depth: 0 }),
        ...normalizeDepths(members).map(toSchemaNode),
      ]),
      // 区分定義・値オブジェクトのノードの深さは画面上のデータ構造によって決まるため、ここで確定させる
      ...project.staticEnums.flatMap(({ root, values }) => [
        toSchemaNode({ ...root, depth: 0 }),
        ...values.map(value => toSchemaNode({ ...value, depth: 1 })),
      ]),
      ...project.dynamicEnumTypes.map(node => toSchemaNode({ ...node, depth: 0 })),
      ...project.valueObjects.map(node => toSchemaNode({ ...node, depth: 0 })),
    ],
    graphLayout: project.graphLayout,
  }
}

/**
 * 画面上で編集したデータを画面初期表示時と同じデータ構造に変換する。
 * toEditingProject の逆変換。
 */
export function toInitialLoadData(project: EditingProject): InitialLoadData {
  const { config, aggregates, graphLayout } = toClientRequest(project)
  return {
    projectRoot: project.projectRoot,
    editingXmlFilePath: project.editingXmlFilePath,
    config,
    aggregates,
    aggregateOrMemberTypes: project.aggregateOrMemberTypes,
    optionalAttributes: project.optionalAttributes,
    graphLayout,
  }
}

/**
 * 設定が読み込めなかった場合に使う、スキーマ定義のルート要素の設定の既定値。
 * 既定値はサーバー側の設定クラスのものと合わせている。
 */
function createDefaultConfig(): Config {
  return {
    RootNamespace: "",
    GenerateUnusedRefToModules: false,
    DbContextName: "MyDbContext",
    CreateUserDbColumnName: null,
    UpdateUserDbColumnName: null,
    CreatedAtDbColumnName: null,
    UpdatedAtDbColumnName: null,
    VersionDbColumnName: null,
    MaxFileSizeMB: null,
    MaxTotalFileSizeMB: null,
    AttachmentFileExtensions: null,
    DisableLocalRepository: false,
    UseBatchUpdateVersion2: false,
    ButtonColor: null,
    UseWijmo: false,
    VFormRefItemIsNotWide: false,
    MultiViewDetailLinkBehavior: "navigateToEditMode",
    VFormMaxColumnCount: null,
    VFormMaxMemberCount: null,
    VFormThreshold: null,
  }
}

/**
 * 画面上で新しく追加するノードを作成する。
 */
export function createNewSchemaNode(depth: number, type?: string): EditingSchemaNode {
  return {
    depth,
    uniqueId: crypto.randomUUID(),
    displayName: "",
    type,
    attrs: {},
  }
}

/**
 * サーバー側のノードを画面上で編集するノードに変換する。
 * 真偽値型の属性は、値を持たないことで指定ありを表すサーバー側の形式から、画面上で扱いやすい真偽値に置き換える。
 */
function toEditingSchemaNode({ attrValues, ...rest }: SchemaNode, attrDefs: Map<string, OptionalAttributeDef>): EditingSchemaNode {
  const attrs: EditingSchemaNode["attrs"] = {}
  for (const { key, value } of attrValues ?? []) {
    if (!key) continue
    attrs[key] = attrDefs.get(key)?.type === "boolean" ? true : value ?? ""
  }
  return { ...rest, attrs }
}

/**
 * 画面上で編集したノードをサーバー側のノードに変換する。
 * 値が空の文字列型・数値型の属性は、画面上で値が消されたものとみなして指定なしにする。
 */
function toSchemaNode({ attrs, ...rest }: EditingSchemaNode): SchemaNode {
  const attrValues: OptionalAttributeValue[] = []
  for (const [key, value] of Object.entries(attrs)) {
    if (typeof value === "boolean") {
      if (value) attrValues.push({ key, value: null })
    } else if (value.trim() !== "") {
      attrValues.push({ key, value })
    }
  }
  return { ...rest, attrValues }
}

/**
 * ルート要素の子孫の深さを、1以上かつ直前の行の深さ + 1 以下になるよう補正する。
 */
function normalizeDepths(members: EditingSchemaNode[]): EditingSchemaNode[] {
  let previousDepth = 0
  return members.map(member => {
    const depth = Math.min(Math.max(member.depth, 1), previousDepth + 1)
    previousDepth = depth
    return depth === member.depth ? member : { ...member, depth }
  })
}

/**
 * 深さ付きのフラットな配列を、ルート要素とその子孫のまとまりごとに分割する。
 * 各まとまりの先頭要素がルート要素。
 */
function splitByRoot(nodes: EditingSchemaNode[]): [EditingSchemaNode, ...EditingSchemaNode[]][] {
  const groups: [EditingSchemaNode, ...EditingSchemaNode[]][] = []
  for (const node of nodes) {
    const current = groups.at(-1)
    if (node.depth === 0 || current === undefined) {
      groups.push([node])
    } else {
      current.push(node)
    }
  }
  return groups
}
