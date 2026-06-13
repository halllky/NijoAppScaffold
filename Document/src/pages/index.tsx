import type { ReactNode } from 'react';
import { Redirect } from '@docusaurus/router';
import useBaseUrl from '@docusaurus/useBaseUrl';

// 「はじめに」(docs/01_intro) へリダイレクト
export default function Home(): ReactNode {
  return <Redirect to={useBaseUrl('/docs/intro')} />;
}
