import React, { createContext, useContext, useState, ReactNode } from 'react';
import * as ReactHookForm from 'react-hook-form';
import { ATTR_IS_QUERYABLE, ATTR_IS_STORED, EditingProject } from '../backend';
import { fetchTypeCandidates } from '../backend/api';

type CandidateItem = { value: string; text: string };

type SchemaCandidatesContextType = {
  isLoading: boolean;
  items: CandidateItem[];
};

const SchemaCandidatesContext = createContext<SchemaCandidatesContextType>({ isLoading: false, items: [] });

/**
 * ノード種類の候補を提供するコンテキストプロバイダ。
 * フォームの内容に応じてサーバーから候補を取得し、コンテキスト経由で提供する。
 */
export const SchemaCandidatesProvider = ({ watch, children }: {
  watch: ReactHookForm.UseFormWatch<EditingProject>,
  children: ReactNode
}) => {
  // フォーム全体の値
  const watchedValues = watch();
  // コンテキスト値
  const [items, setItems] = useState<CandidateItem[]>([]);
  const [isLoading, setIsLoading] = useState(false);
  const abortControllerRef = React.useRef<AbortController | null>(null);

  const contextValue = React.useMemo((): SchemaCandidatesContextType => ({
    isLoading,
    items,
  }), [isLoading, items]);

  // 候補をリロードする関数。現在のフォームデータを引数に受け取る。
  const reloadCandidates = async (): Promise<void> => {
    abortControllerRef.current?.abort();
    const abortController = new AbortController();
    abortControllerRef.current = abortController;
    setIsLoading(true);
    try {
      const result = await fetchTypeCandidates(watchedValues, abortController.signal);
      if (abortController.signal.aborted) {
        return;
      }
      if (!result.ok) {
        console.error('Failed to fetch candidates:', result.error);
        return;
      }
      setItems(result.value);

    } finally {
      if (abortControllerRef.current === abortController) {
        abortControllerRef.current = null;
        setIsLoading(false);
      }
    }
  };

  // 処理の最適化のため、以下のいずれかの情報が変わった時のみ候補のリロードをトリガーする
  // * ルート集約（データ構造・コマンド）の数
  // * ルート集約のいずれかの IsStored, IsQueryable 属性・名前
  // * ルート集約直下の child, children いずれかの名前
  const [triggerValue, setTriggerValue] = useState<unknown[]>([]);
  React.useEffect(() => {
    const roots = [...(watchedValues.dataStructures ?? []), ...(watchedValues.commands ?? [])];
    const newTriggerValue = [
      watchedValues.dataStructures?.length ?? 0,
      watchedValues.commands?.length ?? 0,
      ...roots.flatMap(root => [
        root.attributes[ATTR_IS_STORED],
        root.attributes[ATTR_IS_QUERYABLE],
        root.physicalName,
        ...root.members
          .filter(m => m.type.kind === 'child' || m.type.kind === 'children')
          .map(m => m.physicalName),
      ]),
    ];
    const isChanged = newTriggerValue.length !== triggerValue.length ||
      newTriggerValue.some((v, i) => v !== triggerValue[i]);
    if (isChanged) {
      setTriggerValue(newTriggerValue);
      reloadCandidates();
    }
  }, [watchedValues]);

  return (
    <SchemaCandidatesContext.Provider value={contextValue}>
      {children}
    </SchemaCandidatesContext.Provider>
  );
};

export const useSchemaCandidates = () => {
  return useContext(SchemaCandidatesContext);
};
