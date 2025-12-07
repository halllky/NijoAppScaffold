// Helper to determine language for syntax highlighting
export function getLanguage(filename: string): string {
  const ext = filename.split('.').pop()?.toLowerCase();
  switch (ext) {
    case 'ts': case 'tsx': return 'typescript';
    case 'js': case 'jsx': return 'javascript';
    case 'cs': return 'csharp';
    case 'xml': case 'csproj': return 'xml';
    case 'json': return 'json';
    case 'md': return 'markdown';
    case 'css': return 'css';
    case 'html': return 'html';
    case 'sql': return 'sql';
    case 'sh': case 'bash': return 'bash';
    case 'bat': return 'batch';
    default: return 'text';
  }
}
