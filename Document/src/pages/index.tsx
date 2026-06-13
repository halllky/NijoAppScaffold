import type { ReactNode } from 'react';
import { Redirect } from '@docusaurus/router';
import useBaseUrl from '@docusaurus/useBaseUrl';

// 「はじめに」へリダイレクト
export default function Home(): ReactNode {
  return <Redirect to={useBaseUrl('/docs/intro')} />;
}
