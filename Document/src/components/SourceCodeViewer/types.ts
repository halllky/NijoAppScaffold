export type SourceMap = Record<string, { code: string }>;

export type TreeNode = {
  name: string;
  path: string;
  type: 'file' | 'folder';
  children: TreeNode[];
  description?: string;
};
