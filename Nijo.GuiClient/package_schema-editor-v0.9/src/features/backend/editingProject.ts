// サーバーとやり取りするデータ構造と、画面上で編集するためのデータ構造との相互変換。
// サーバー側はすべてのノードを深さ付きのフラットな配列で扱うが、
// 画面側では編集単位ごとに配列を分けておいた方がグリッド等にそのままバインドできて扱いやすいため、ここで組み替える。

import type { ClientRequest, Config, InitialLoadData, OptionalAttributeDef, OptionalAttributeValue, SchemaNode, SchemaNodeTypeDef } from "./types"

/** 画面上で編集するプロジェクトのデータ */
export type EditingProject = {
  projectRoot?: string | null
  editingXmlFilePath?: string | null
  config?: Config | null
  /** 区分定義以外のルート要素とその子孫 */
  rootAggregates: RootAggregateDef[]
  /** 静的区分（列挙体）の定義 */
  staticEnums: StaticEnumDef[]
  /** 動的区分（区分マスタ）の種類。いずれも子要素を持たないルート要素 */
  dynamicEnumTypes: EditingSchemaNode[]
  aggregateOrMemberTypes: SchemaNodeTypeDef[]
  optionalAttributes: OptionalAttributeDef[]
}

/**
 * 画面上で編集する集約または集約メンバー。
 * オプショナル属性を属性のキーで引けるようにしたこと以外はサーバー側の構造と同じ。
 */
export type EditingSchemaNode = Omit<SchemaNode, "attrValues"> & {
  /**
   * オプショナル属性の値。キーは属性のキー。
   * キーが存在すればその属性が指定されていることを表す。真偽値型の属性の値は空文字。
   */
  attrs: { [key: string]: string }
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

/** 静的区分のルート要素の種類 */
export const NODE_TYPE_STATIC_ENUM = "enum"
/** 動的区分の種類のルート要素の種類 */
export const NODE_TYPE_DYNAMIC_ENUM_TYPE = "dynamic-enum-type"

/** Write Model のルート要素の種類 */
export const NODE_TYPE_WRITE_MODEL = "write-model-2"
/** Read Model のルート要素の種類 */
export const NODE_TYPE_READ_MODEL = "read-model-2"
/** Write Model と Read Model の両方を兼ねるルート要素の種類 */
export const NODE_TYPE_WRITE_READ_MODEL = "write-model-2 generate-default-read-model"
/** Command Model のルート要素の種類 */
export const NODE_TYPE_COMMAND_MODEL = "command"
/** 値オブジェクトのルート要素の種類 */
export const NODE_TYPE_VALUE_OBJECT = "value-object"
/** 親と1対1の関係を持つ子集約の種類 */
export const NODE_TYPE_CHILD = "child"
/** 親と1対多の関係を持つ子集約の種類 */
export const NODE_TYPE_CHILDREN = "children"

/**
 * 他の集約への参照を表すメンバーの種類の接頭辞。
 * 種類の値はこの接頭辞の後ろに参照先ノードの uniqueId が続く形になる。
 */
export const NODE_TYPE_PREFIX_REF_TO = "ref-to:"

/** 物理名のオプショナル属性のキー。未指定の場合は displayName から物理名が決まる */
export const ATTR_KEY_PHYSICAL_NAME = "physical-name"

/**
 * サーバーから受け取ったデータを画面上で編集するためのデータ構造に変換する。
 */
export function toEditingProject(data: InitialLoadData): EditingProject {
  const rootAggregates: RootAggregateDef[] = []
  const staticEnums: StaticEnumDef[] = []
  const dynamicEnumTypes: EditingSchemaNode[] = []

  // ルート要素ごとに、その種類に応じた振り分け先へ子孫ごと移す
  const nodes = (data.aggregates ?? []).map(toEditingSchemaNode)
  for (const [root, ...descendants] of splitByRoot(nodes)) {
    if (root.type === NODE_TYPE_STATIC_ENUM) {
      staticEnums.push({ root, values: descendants })
    } else if (root.type === NODE_TYPE_DYNAMIC_ENUM_TYPE) {
      dynamicEnumTypes.push(root)
    } else {
      rootAggregates.push({ root, members: descendants })
    }
  }

  return {
    projectRoot: data.projectRoot,
    editingXmlFilePath: data.editingXmlFilePath,
    config: data.config,
    rootAggregates,
    staticEnums,
    dynamicEnumTypes,
    aggregateOrMemberTypes: data.aggregateOrMemberTypes ?? [],
    optionalAttributes: data.optionalAttributes ?? [],
  }
}

/**
 * 画面上で編集したデータをサーバーに送るデータ構造に変換する。
 * ルート要素の並び順は、集約、静的区分、動的区分の種類の順になる。
 * 画面上の編集操作によって深さの連続性が崩れている場合、ここで補正する。
 */
export function toClientRequest(project: EditingProject): ClientRequest {
  const attrDefs = new Map(project.optionalAttributes.map(def => [def.key, def]))
  const toServer = (node: EditingSchemaNode) => toSchemaNode(node, attrDefs)

  return {
    config: project.config,
    aggregates: [
      ...project.rootAggregates.flatMap(({ root, members }) => [
        toServer({ ...root, depth: 0 }),
        ...normalizeDepths(members).map(toServer),
      ]),
      // 区分定義のノードの深さは画面上のデータ構造によって決まるため、ここで確定させる
      ...project.staticEnums.flatMap(({ root, values }) => [
        toServer({ ...root, depth: 0 }),
        ...values.map(value => toServer({ ...value, depth: 1 })),
      ]),
      ...project.dynamicEnumTypes.map(node => toServer({ ...node, depth: 0 })),
    ],
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
 */
function toEditingSchemaNode({ attrValues, ...rest }: SchemaNode): EditingSchemaNode {
  const attrs: EditingSchemaNode["attrs"] = {}
  for (const { key, value } of attrValues ?? []) {
    if (key) attrs[key] = value ?? ""
  }
  return { ...rest, attrs }
}

/**
 * 画面上で編集したノードをサーバー側のノードに変換する。
 * 値が空の文字列型・数値型の属性は、画面上で値が消されたものとみなして指定なしにする。
 */
function toSchemaNode({ attrs, ...rest }: EditingSchemaNode, attrDefs: Map<string, OptionalAttributeDef>): SchemaNode {
  const attrValues: OptionalAttributeValue[] = []
  for (const [key, value] of Object.entries(attrs)) {
    const isBoolean = attrDefs.get(key)?.type === "boolean"
    if (value.trim() === "") {
      if (isBoolean) attrValues.push({ key, value: null })
    } else {
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
