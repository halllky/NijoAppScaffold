import { TreeNode } from "./types";
import React from 'react';

export interface FileTreeItemProps {
  node: TreeNode;
  level: number;
  activeFile: string | null;
  onSelect: (path: string) => void;
  expandedFolders: Set<string>;
  toggleFolder: (path: string) => void;
  renderRow: (node: TreeNode, level: number, isActive: boolean, isExpanded: boolean) => React.ReactNode;
}

export const FileTreeItem = ({
  node,
  level,
  activeFile,
  onSelect,
  expandedFolders,
  toggleFolder,
  renderRow
}: FileTreeItemProps) => {
  const isExpanded = expandedFolders.has(node.path);
  const isActive = node.path === activeFile;

  const handleClick = () => {
    if (node.type === 'folder') {
      toggleFolder(node.path);
    } else {
      onSelect(node.path);
    }
  };

  return (
    <div>
      <div
        onClick={handleClick}
        style={{
          cursor: 'pointer',
          backgroundColor: isActive ? 'var(--ifm-color-emphasis-100)' : 'transparent',
        }}
        className="file-tree-item"
      >
        {renderRow(node, level, isActive, isExpanded)}
      </div>
      {node.type === 'folder' && isExpanded && (
        <div>
          {node.children.map(child => (
            <FileTreeItem
              key={child.path}
              node={child}
              level={level + 1}
              activeFile={activeFile}
              onSelect={onSelect}
              expandedFolders={expandedFolders}
              toggleFolder={toggleFolder}
              renderRow={renderRow}
            />
          ))}
        </div>
      )}
    </div>
  );
};
