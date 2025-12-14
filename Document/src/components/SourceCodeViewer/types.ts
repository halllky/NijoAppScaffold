export type SourceMap = Record<string, { size: number }>;

export type TreeNode = {
  name: string;
  path: string;
  type: 'file' | 'folder';
  children: TreeNode[];
  description?: string;
};
