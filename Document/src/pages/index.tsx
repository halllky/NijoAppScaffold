import type { ReactNode } from 'react';
import Link from '@docusaurus/Link';
import useBaseUrl from '@docusaurus/useBaseUrl';
import useDocusaurusContext from '@docusaurus/useDocusaurusContext';
import Layout from '@theme/Layout';
import Heading from '@theme/Heading';

import styles from './index.module.css';

type Point = {
  title: string;
  body: string;
};

const salesPoints: Point[] = [
  {
    title: 'フルスタック',
    body:
      '画面からデータベースまでを対象範囲にしつつ、一覧検索のような定型処理を自動生成して、業務固有の表現だけを個別実装に残せます。',
  },
  {
    title: '型安全',
    body:
      'C# と TypeScript の型定義を同じスキーマから生成し、Entity Framework Core まで含めてデータ構造の同期を保ちます。',
  },
  {
    title: '設計共有',
    body:
      'スキーマ定義をGUIで可視化し、複雑なリレーションや機能分割を関係者と俯瞰しながら議論できます。',
  },
  {
    title: '業務システム向け',
    body:
      '排他制御、論理削除、監査用属性、ログマスキングのような基幹系で問題になりやすい論点を自然に組み込むことができます。',
  },
  {
    title: 'スクラッチ並みの柔軟性',
    body:
      'UI、認証認可、外部連携、複雑な業務ロジックは通常の C# と TypeScript で自由に作り込めます。',
  },
];

function HeroSection(): ReactNode {
  return (
    <section className={styles.heroSection}>
      <div className={styles.heroInner}>
        <div className={styles.heroCopy}>
          <p className={styles.eyebrow}>Schema-driven full-stack application scaffold</p>
          <Heading as="h1" className={styles.heroTitle}>
            Nijoは、業務システムの
            <br />
            画面からDBまでをつなぐ
            <br />
            スキーマ駆動の開発基盤です。
          </Heading>
          <p className={styles.heroLead}>
            開発者が主に操作するのはスキーマ定義GUIです。
            そこで定義した構造をもとに、C#、TypeScript、Entity Framework Core のコードを生成し、
            型の整合と設計共有を同時に前へ進めます。
          </p>
          <div className={styles.heroActions}>
            <Link className="button button--primary button--lg" to="/docs/intro">
              ドキュメントを読む
            </Link>
            <Link className={styles.ghostButton} to="https://github.com/halllky/NijoAppScaffold">
              GitHubを見る
            </Link>
          </div>
          <div className={styles.heroNotes}>
            <span>開発者が主に操作するのはスキーマ定義GUI</span>
            <span>画面からDBまでをスコープに含む</span>
            <span>UIは自由に作り込める</span>
          </div>
        </div>
      </div>
    </section>
  );
}

function Home(): ReactNode {
  const { siteConfig } = useDocusaurusContext();
  const guiEditorImage = useBaseUrl('/img/gui-sample-2.png');
  const businessUiImage = useBaseUrl('/img/lp-003.png');
  const boundaryImage = useBaseUrl('/img/lp-001.png');
  const typeSyncImage = useBaseUrl('/img/lp-002.png');

  return (
    <Layout
      title={`${siteConfig.title} | フルスタック・スキーマ駆動アプリケーション基盤`}
      description="Nijoは、業務システムの画面からデータベースまでを対象に、C#、TypeScript、Entity Framework Core のコードをスキーマから生成する開発基盤です。">
      <HeroSection />

      <main className={styles.pageMain}>
        <section className={styles.sectionBlock}>
          <div className={styles.sectionHeading}>
            <p className={styles.sectionLabel}>Scope</p>
            <Heading as="h2">何を自動化し、何を開発者に残すかが明確です。</Heading>
            <p>
              Nijoは「何でも自動化する」ツールではありません。スキーマから一意に決まる部分を強く自動化し、
              業務価値が出る部分は通常のスクラッチ開発と同じ自由度で残します。
            </p>
          </div>

          <div className={styles.imageTextStack}>
            <div className={styles.copyCard}>
              <Heading as="h3">設計の中心はスキーマ定義</Heading>
              <p>
                画面項目、データ構造、参照関係、更新単位をスキーマとして定義し、関係者が同じ絵を見ながら議論できます。
                この可視化は、あとから陳腐化する資料ではなく、コード生成と同期された開発中の設計図として機能します。
              </p>
              <p>
                開発者は主にこのGUIを操作して、データ構造や関係性を定義します。
              </p>
              <img src={guiEditorImage} alt="Nijoのスキーマ定義GUI画面" />
            </div>
          </div>

          <div className={styles.imageTextStack} style={{ marginTop: '1rem' }}>
            <div className={styles.copyCard}>
              <Heading as="h3">対象範囲は画面からDBまで</Heading>
              <p>
                Nijoは、Webクライアント、サーバー、データベースまでをひと続きの構造として扱います。
                一覧検索処理のような定型ロジックは自動生成できますが、検索条件や検索結果を画面上でどう表現するかは、
                通常のJavaScriptやTypeScriptによる開発で自由に決められます。
              </p>
              <p>
                自動生成は下支えに徹し、エンドユーザー向けUIは案件に合わせて作り込めます。
              </p>
              <img src={businessUiImage} alt="Nijoで構築された販売管理システムの画面" />
            </div>
          </div>
        </section>

        <section className={styles.sectionBlockAlt}>
          <div className={styles.sectionHeading}>
            <p className={styles.sectionLabel}>Boundary</p>
            <Heading as="h2">自動生成と個別実装の境界が読み取りやすい構造です。</Heading>
            <p>
              更新系の処理では、共通で守るべき品質と、案件ごとに変わる業務判断を分けて扱うことが重要です。
              Nijoはその境界をコード上にも明示します。
            </p>
          </div>

          <div className={styles.imageTextStack}>
            <div className={styles.copyCard}>
              <Heading as="h3">画像左側: 開発者が自身で実装する部分</Heading>
              <ul>
                <li>
                  ※1 在庫引当のような業務固有ロジック（在庫引当: 小売店などで売上確定時に在庫データのうちどれをその売上と紐づけるかのロジックのこと）
                </li>
                <li>
                  ※2 更新前の確認メッセージを出すタイミングや文言
                </li>
                <li>
                  ※3 トランザクションの境界。（画像中には無いが、UI表現、認証認可、外部システム連携なども個別実装）
                </li>
              </ul>
              <Heading as="h3">画像右側: 自動生成された部分</Heading>
              <ul>
                <li>
                  ※4 楽観排他のバージョンや登録日時・登録者など監査用属性の設定
                </li>
                <li>
                  ※5 必須、MaxLength、桁数、型といったデータ構造から自明に導ける入力チェック
                </li>
              </ul>
              <Heading as="h3">矢印: 手動実装部分で自動生成処理を呼び出している箇所</Heading>
              <img src={boundaryImage} alt="自動生成コードと手動実装コードの境界を示す画面" />
            </div>
          </div>
        </section>

        <section className={styles.sectionBlock}>
          <div className={styles.imageTextStack}>
            <div className={styles.copyCard}>
              <p className={styles.sectionLabel}>Type Safety</p>
              <Heading as="h2">TypeScriptとC#の構造体が自動的に同期されます。</Heading>
              <p>
                サーバー側はC#、クライアント側はTypeScriptで厳密に型定義されたデータクラスを生成します。
                同じスキーマ定義から両方を生成するため、言語を跨いだデータ構造の整合性を維持しやすくなります。
                データベースとアプリケーションの型同期は Entity Framework Core の標準的な仕組みに乗ります。
              </p>
              <p>
                画像左: TypeScript , 画像右: C#
              </p>
              <img src={typeSyncImage} alt="TypeScriptとC#の構造体が同期される例" />
            </div>
          </div>
        </section>

        <section className={styles.sectionBlockDark}>
          <div className={styles.sectionHeadingNarrow}>
            <p className={styles.sectionLabel}>Sales Points</p>
            <Heading as="h2">このツールが解決しようとしている問題</Heading>
            <p>
              画面、API、型定義、DB設計、チーム内の認識合わせが分断されると、単純作業が増え、変更に弱くなります。
              Nijoはその接続部分をスキーマと生成コードで受け持ちます。
            </p>
          </div>
          <div className={styles.pointGrid}>
            {salesPoints.map((point) => (
              <article key={point.title} className={styles.pointCard}>
                <Heading as="h3">{point.title}</Heading>
                <p>{point.body}</p>
              </article>
            ))}
          </div>
        </section>

        <section className={styles.sectionBlock}>
          <div className={styles.sectionHeading}>
            <p className={styles.sectionLabel}>Constraints</p>
            <Heading as="h2">重要な制約を先に明示します。</Heading>
            <p>
              採用前に判断しやすいように、必須となる技術と、Nijoがカバーしない責務をトップページでも明示します。
            </p>
          </div>

          <div className={styles.constraintGrid}>
            <article className={styles.constraintCard}>
              <Heading as="h3">利用必須</Heading>
              <ul>
                <li>C# / .NET</li>
                <li>Entity Framework Core</li>
              </ul>
            </article>
            <article className={styles.constraintCard}>
              <Heading as="h3">利用推奨</Heading>
              <ul>
                <li>ASP.NET Core</li>
                <li>TypeScript</li>
              </ul>
            </article>
            <article className={styles.constraintCard}>
              <Heading as="h3">別途設計が必要</Heading>
              <ul>
                <li>認証・認可</li>
                <li>UIコンポーネントの具体的な挙動</li>
                <li>外部システム連携や横断的な運用要件</li>
              </ul>
            </article>
          </div>

          <div className={styles.bottomCallout}>
            <Heading as="h3">先へ読み進める入口</Heading>
            <p>
              詳細ドキュメントでは、開発工程との統合、自動生成と個別実装の境界、
              nijo.xml を設計ドキュメントとしてどこまで正とみなせるか、アーキテクチャと開発ワークフローを順に説明します。
            </p>
            <Link className="button button--primary button--lg" to="/docs/intro">
              はじめにから読む
            </Link>
          </div>
        </section>
      </main>
    </Layout>
  );
}

export default Home;
