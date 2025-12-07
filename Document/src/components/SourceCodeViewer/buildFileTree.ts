import { SourceMap, TreeNode } from "./types";

// Helper to build tree structure from paths
export function buildFileTree(files: SourceMap, root: string, includes: [string, string][]): TreeNode[] {
  const treeRoot: TreeNode[] = [];

  // includesで指定されたパスを正規化して、説明文とのマップを作成
  const includeMap = new Map<string, string>();
  includes.forEach(([path, desc]) => {
    includeMap.set(path, desc);
  });

  Object.keys(files).forEach(filePath => {
    // Check if file is included
    // includesのいずれかのパスで始まっているか確認
    const matchedInclude = includes.find(([incPath]) => filePath.startsWith(incPath));
    if (!matchedInclude) return;

    // rootからの相対パスを計算
    if (!filePath.startsWith(root)) return; // root外のファイルは無視（通常ありえないが念のため）

    const relativePath = filePath.substring(root.length);
    const parts = relativePath.split('/').filter(p => p); // filter empty strings

    let currentLevel = treeRoot;
    let currentPath = root;

    parts.forEach((part, index) => {
      // Determine if this part is a file or a folder
      // The last part is a file ONLY if the original filePath points to a file in the source map
      // However, we are iterating over keys of `files`, so `filePath` is definitely a file.
      // So the last part is indeed a file.
      const isFile = index === parts.length - 1;

      // Construct path carefully
      if (currentPath === '' || currentPath === '/') {
        currentPath = `/${part}`;
      } else {
        currentPath = `${currentPath}/${part}`;
      }

      // Fix: Ensure path doesn't have double slashes
      currentPath = currentPath.replace('//', '/');

      let node = currentLevel.find(n => n.name === part);

      if (!node) {
        // 説明文を取得
        // ファイルまたはフォルダがincludesに直接指定されている場合、その説明文を使用
        const description = includeMap.get(currentPath);

        node = {
          name: part,
          path: isFile ? filePath : currentPath, // Use original filePath for files
          type: isFile ? 'file' : 'folder',
          children: [],
          description,
        };
        currentLevel.push(node);

        // Sort: folders first, then files, alphabetically
        currentLevel.sort((a, b) => {
          if (a.type === b.type) return a.name.localeCompare(b.name);
          return a.type === 'folder' ? -1 : 1;
        });
      } else {
        // 既存ノードの場合でも、もしそれがフォルダで、かつincludesに指定されていれば説明文を更新する可能性がある
        // (ただし、通常は親フォルダが先に作られるとは限らないので、ここでチェックするのは正しい)
        if (!node.description) {
          const description = includeMap.get(currentPath);
          if (description) {
            node.description = description;
          }
        }
      }

      currentLevel = node.children;
    });
  });

  return treeRoot;
}
