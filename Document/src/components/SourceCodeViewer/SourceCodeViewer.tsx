import React, { useState, useMemo, useEffect, useRef } from 'react';
import CodeBlock from '@theme/CodeBlock';
import useBaseUrl from '@docusaurus/useBaseUrl';
import { Allotment } from 'allotment';
import 'allotment/dist/style.css';
import sourceMap from '../../generated/file-tree.json';
import { buildFileTree } from './buildFileTree';
import { SourceMap, TreeNode } from './types';
import { getLanguage } from './getLanguage';
import { FileTreeItem } from './FileTreeItemWithDescription';
import styles from './SourceCodeViewer.module.css';

export interface SourceCodeViewerProps {
  title?: string;
  root: string;
  includes: [filePath: string, description: string][];
  height?: string;
  defaultExplorerWidth?: number;
}

export default function SourceCodeViewer({ title, root, includes, height = '600px', defaultExplorerWidth = 250 }: SourceCodeViewerProps) {
  // Filter files based on root and includes
  const files = useMemo(() => {
    const filtered: SourceMap = {};
    Object.keys(sourceMap).forEach(key => {
      // rootで始まっているか
      if (!key.startsWith(root)) return;

      // includesのいずれかで始まっているか
      const isIncluded = includes.some(([incPath]) => key.startsWith(incPath));
      if (isIncluded) {
        filtered[key] = (sourceMap as any)[key];
      }
    });
    return filtered;
  }, [root, includes]);

  // Build tree structure
  const tree = useMemo(() => buildFileTree(files, root, includes), [files, root, includes]);

  // State
  const [activeFile, setActiveFile] = useState<string | null>(null);
  const [fileContent, setFileContent] = useState<string>('');
  const [isLoading, setIsLoading] = useState<boolean>(false);
  const [error, setError] = useState<string | null>(null);

  // Cache
  const contentCache = useRef<Record<string, string>>({});

  const baseUrl = useBaseUrl('/');

  // Fetch content
  useEffect(() => {
    if (!activeFile) {
      setFileContent('');
      return;
    }

    if (contentCache.current[activeFile]) {
      setFileContent(contentCache.current[activeFile]);
      setIsLoading(false);
      setError(null);
      return;
    }

    setIsLoading(true);
    setError(null);
    setFileContent('');

    // activeFile starts with /, e.g. /demo/folder/file.ts
    // We want to fetch /source-code/demo/folder/file.ts
    // useBaseUrl handles the site base URL.
    // Note: baseUrl usually ends with /
    const url = `${baseUrl}source-code${activeFile}`;

    let isMounted = true;

    fetch(url)
      .then(res => {
        if (!res.ok) throw new Error(`Failed to load file: ${res.statusText}`);
        return res.text();
      })
      .then(text => {
        if (isMounted) {
          contentCache.current[activeFile] = text;
          setFileContent(text);
          setIsLoading(false);
        }
      })
      .catch(err => {
        if (isMounted) {
          console.error(err);
          setError(err.message);
          setFileContent(`// Error loading file: ${err.message}`);
          setIsLoading(false);
        }
      });

    return () => {
      isMounted = false;
    };
  }, [activeFile, baseUrl]);

  // 初期表示時にすべてのフォルダを展開する
  const [expandedFolders, setExpandedFolders] = useState<Set<string>>(new Set());

  useEffect(() => {
    const allFolders = new Set<string>();
    const traverse = (nodes: TreeNode[]) => {
      nodes.forEach(node => {
        if (node.type === 'folder') {
          allFolders.add(node.path);
          traverse(node.children);
        }
      });
    };
    traverse(tree);
    setExpandedFolders(allFolders);
  }, [tree]);

  const toggleFolder = (path: string) => {
    const next = new Set(expandedFolders);
    if (next.has(path)) {
      next.delete(path);
    } else {
      next.add(path);
    }
    setExpandedFolders(next);
  };

  const activeLanguage = activeFile ? getLanguage(activeFile) : 'text';

  // Scroll synchronization
  const treeContainerRef = useRef<HTMLDivElement>(null);
  const descContainerRef = useRef<HTMLDivElement>(null);
  const isScrollingRef = useRef(false);

  useEffect(() => {
    if (activeFile) return; // Only sync when description pane is visible

    const treeEl = treeContainerRef.current;
    const descEl = descContainerRef.current;
    if (!treeEl || !descEl) return;

    const handleScroll = (source: HTMLElement, target: HTMLElement) => {
      if (isScrollingRef.current) return;
      isScrollingRef.current = true;
      target.scrollTop = source.scrollTop;
      setTimeout(() => { isScrollingRef.current = false; }, 50);
    };

    const onTreeScroll = () => handleScroll(treeEl, descEl);
    const onDescScroll = () => handleScroll(descEl, treeEl);

    treeEl.addEventListener('scroll', onTreeScroll);
    descEl.addEventListener('scroll', onDescScroll);

    return () => {
      treeEl.removeEventListener('scroll', onTreeScroll);
      descEl.removeEventListener('scroll', onDescScroll);
    };
  }, [activeFile]);

  const renderNameRow = (node: TreeNode, level: number, isActive: boolean, isExpanded: boolean) => (
    <div style={{
      paddingLeft: `${level * 12 + 8}px`,
      paddingRight: '8px',
      paddingTop: '4px',
      paddingBottom: '4px',
      display: 'flex',
      alignItems: 'center',
      fontSize: '13px',
      color: isActive ? 'var(--ifm-color-primary)' : 'var(--ifm-color-content)',
      whiteSpace: 'nowrap',
      height: '28px',
      boxSizing: 'border-box'
    }}>
      <span style={{ marginRight: '6px', opacity: 0.7, fontSize: '14px' }}>
        {node.type === 'folder' ? (isExpanded ? '📂' : '📁') : '📄'}
      </span>
      <span style={{ overflow: 'hidden', textOverflow: 'ellipsis' }}>{node.name}</span>
    </div>
  );

  const renderDescriptionRow = (node: TreeNode, level: number, isActive: boolean, isExpanded: boolean) => (
    <div style={{
      paddingLeft: '8px',
      paddingRight: '8px',
      paddingTop: '4px',
      paddingBottom: '4px',
      display: 'flex',
      alignItems: 'center',
      fontSize: '12px',
      color: 'var(--ifm-color-content-secondary)',
      whiteSpace: 'nowrap',
      height: '28px',
      boxSizing: 'border-box',
      borderBottom: '1px solid var(--ifm-color-emphasis-200)'
    }}>
      {node.description || ''}
    </div>
  );

  return (
    <div style={{
      border: '1px solid var(--ifm-color-emphasis-200)',
      borderRadius: 'var(--ifm-global-radius)',
      height,
      display: 'flex',
      overflow: 'hidden',
      backgroundColor: 'var(--ifm-background-surface-color)'
    }}>
      <Allotment>
        <Allotment.Pane
          preferredSize={activeFile ? defaultExplorerWidth : '50%'}
          minSize={150}
        >
          {/* File Explorer (Names) */}
          <div style={{
            height: '100%',
            display: 'flex',
            flexDirection: 'column',
            backgroundColor: 'var(--ifm-card-background-color)',
            overflow: 'hidden'
          }}>
            <div style={{
              padding: '8px 12px',
              fontWeight: 'bold',
              fontSize: '12px',
              textTransform: 'uppercase',
              color: 'var(--ifm-color-emphasis-600)',
              borderBottom: '1px solid var(--ifm-color-emphasis-200)',
              position: 'sticky',
              top: 0,
              backgroundColor: 'inherit',
              zIndex: 1,
              display: 'flex',
              justifyContent: 'space-between',
              height: '33px', // Fixed height to match description header
              boxSizing: 'border-box'
            }}>
              <span>{title || 'Explorer'}</span>
            </div>
            <div
              ref={treeContainerRef}
              style={{ paddingBottom: '10px', overflow: 'auto', flex: 1 }}
            >
              {tree.map(node => (
                <FileTreeItem
                  key={node.path}
                  node={node}
                  level={0}
                  activeFile={activeFile || ''}
                  onSelect={setActiveFile}
                  expandedFolders={expandedFolders}
                  toggleFolder={toggleFolder}
                  renderRow={renderNameRow}
                />
              ))}
            </div>
          </div>
        </Allotment.Pane>

        <Allotment.Pane>
          {!activeFile ? (
            /* Description View */
            <div style={{
              height: '100%',
              display: 'flex',
              flexDirection: 'column',
              backgroundColor: 'var(--ifm-card-background-color)',
              overflow: 'hidden',
              borderLeft: '1px solid var(--ifm-color-emphasis-200)'
            }}>
              <div style={{
                padding: '8px 12px',
                fontWeight: 'bold',
                fontSize: '12px',
                textTransform: 'uppercase',
                color: 'var(--ifm-color-emphasis-600)',
                borderBottom: '1px solid var(--ifm-color-emphasis-200)',
                position: 'sticky',
                top: 0,
                backgroundColor: 'inherit',
                zIndex: 1,
                height: '33px',
                boxSizing: 'border-box'
              }}>
                Description
              </div>
              <div
                ref={descContainerRef}
                style={{ paddingBottom: '10px', overflow: 'auto', flex: 1 }}
              >
                {tree.map(node => (
                  <FileTreeItem
                    key={node.path}
                    node={node}
                    level={0}
                    activeFile={activeFile || ''}
                    onSelect={setActiveFile}
                    expandedFolders={expandedFolders}
                    toggleFolder={toggleFolder}
                    renderRow={renderDescriptionRow}
                  />
                ))}
              </div>
            </div>
          ) : (
            /* Code Editor */
            <div style={{
              height: '100%',
              display: 'flex',
              flexDirection: 'column',
              overflow: 'hidden'
            }}>
              <div style={{
                padding: '8px 16px',
                borderBottom: '1px solid var(--ifm-color-emphasis-200)',
                fontSize: '13px',
                color: 'var(--ifm-color-content-secondary)',
                backgroundColor: 'var(--ifm-card-background-color)',
                display: 'flex',
                alignItems: 'center',
                height: '33px',
                boxSizing: 'border-box'
              }}>
                <span style={{ marginRight: '8px' }}>📄</span>
                {activeFile.split('/').pop()}
                <div style={{ flex: 1 }}></div>
                <button
                  onClick={() => setActiveFile(null)}
                  style={{
                    background: 'none',
                    border: 'none',
                    cursor: 'pointer',
                    fontSize: '16px',
                    padding: 0,
                    lineHeight: 1,
                    color: 'var(--ifm-color-emphasis-600)'
                  }}
                  title="Close file"
                >
                  ×
                </button>
              </div>
              <div style={{
                flex: 1,
                overflow: 'auto',
                margin: 0,
              }}>
                <CodeBlock
                  language={activeLanguage}
                  showLineNumbers
                  className={styles.sourceCodeViewerBlock}
                >
                  {isLoading ? '// Loading...' : (error ? `// Error: ${error}` : fileContent)}
                </CodeBlock>
              </div>
            </div>
          )}
        </Allotment.Pane>
      </Allotment>
    </div>
  );
}
