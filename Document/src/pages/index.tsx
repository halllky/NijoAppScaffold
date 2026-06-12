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
      '画面からデータベースまでを一体で扱います。一覧検索のような定型処理は自動生成されるので、手で書くのは業務固有の部分だけです。',
  },
  {
    title: '型安全',
    body:
      'C# と TypeScript の型定義を同じスキーマから生成します。Entity Framework Core まで含めて、データ構造のずれが起きません。',
  },
  {
    title: '設計共有',
    body:
      'スキーマ定義はGUIで図として見られるので、複雑なリレーションや機能分割をチームで俯瞰しながら議論できます。',
  },
  {
    title: '業務システム向け',
    body:
      '排他制御、論理削除、監査用属性、ログマスキングといった、基幹系でつまずきやすい論点を最初から考慮しています。',
  },
  {
    title: 'スクラッチ並みの柔軟性',
    body:
      'UI、認証認可、外部連携、複雑な業務ロジックは、通常の C# と TypeScript で自由に作り込めます。',
  },
];

function HeroSection(): ReactNode {
  return (
    <section className={styles.heroSection}>
      <div className={styles.heroInner}>
        <div className={styles.heroCopy}>
          <p className={styles.eyebrow}>Schema-driven full-stack application scaffold</p>
          <Heading as="h1" className={styles.heroTitle}>
            業務システムの“つなぎ”を、書かない。
          </Heading>
          <p className={styles.heroLead}>
            画面、API、型定義、DBのあいだを埋める単純作業は、システムの価値を生みません。
            Nijoは、GUIで定義したスキーマから C#・TypeScript・EF Core のコードを生成し、
            開発者が書くのを業務固有のロジックだけにする開発基盤です。
          </p>
          <div className={styles.heroActions}>
            <Link className="button button--primary button--lg" to="/docs/intro">
              ドキュメントを読む
            </Link>
            <Link className={styles.ghostButton} to="https://github.com/halllky/NijoAppScaffold">
              GitHubを見る
            </Link>
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
            <p className={styles.sectionLabel}>Problem</p>
            <Heading as="h2">同じデータ構造を、何度も書いていませんか</Heading>
            <p>
              たとえば「商品」というひとつの概念のために、テーブル定義、C#のエンティティ、APIの入出力、
              TypeScriptの型、画面のフォーム、そして設計書を書く。
              業務システムの開発では、本質的に同じ構造を形を変えて何度も記述し、
              仕様変更のたびにそのすべてを揃え直すことになります。
              Nijoが「つなぎ」と呼んでなくそうとしているのは、この作業です。
            </p>
          </div>
        </section>

        <section className={styles.sectionBlockAlt}>
          <div className={styles.sectionHeading}>
            <p className={styles.sectionLabel}>How it works</p>
            <Heading as="h2">スキーマを1回定義して、業務ロジックだけを書く</Heading>
            <p>
              Nijoでの開発は、おおまかに次の流れで進みます。
            </p>
          </div>

          <div className={styles.imageTextStack}>
            <div className={styles.copyCard}>
              <Heading as="h3">1. スキーマをGUIで定義する</Heading>
              <p>
                画面項目、データ構造、参照関係、更新単位をGUIで定義します。
                この定義がコード生成の唯一の入力になるため、設計書と実装が食い違ったまま放置されることがありません。
                複雑なリレーションも図として俯瞰できるので、チームでの認識合わせにもそのまま使えます。
              </p>
              <img src={guiEditorImage} alt="Nijoのスキーマ定義GUI画面" />
            </div>
          </div>

          <div className={styles.imageTextStack} style={{ marginTop: '1rem' }}>
            <div className={styles.copyCard}>
              <Heading as="h3">2. 画面からDBまでのコードが生成される</Heading>
              <p>
                スキーマから、サーバー側のC#クラス、クライアント側のTypeScript型定義、
                Entity Framework Core のマッピングが生成されます。
                一覧検索や登録・更新・削除といった定型処理も同様です。
                言語をまたいでデータ構造がずれることはなく、スキーマを変更すれば全体に反映されます。
              </p>
              <p>
                左がTypeScript、右がC#のコードです。
              </p>
              <img src={typeSyncImage} alt="TypeScriptとC#の構造体が同期される例" />
            </div>
          </div>

          <div className={styles.imageTextStack} style={{ marginTop: '1rem' }}>
            <div className={styles.copyCard}>
              <Heading as="h3">3. 業務固有のロジックだけを手で書く</Heading>
              <p>
                生成されたコードには、開発者が書き込むための場所が空けてあります。
                次の画像では、左が手書きのコード、右が自動生成されたコードです。
              </p>
              <p>手で書く部分（画像左側）:</p>
              <ul>
                <li>
                  ※1: 業務固有のロジック。この例では在庫引当（売上確定時に、どの在庫データをその売上と紐づけるかを決める処理）
                </li>
                <li>
                  ※2: 更新前の確認メッセージを出すタイミングや文言
                </li>
                <li>
                  ※3: トランザクションの境界。このほか、UI表現、認証認可、外部システム連携なども手書きの範囲です
                </li>
              </ul>
              <p>自動生成される部分（画像右側）:</p>
              <ul>
                <li>
                  ※4: 楽観排他のバージョンや、登録日時・登録者といった監査用属性の設定
                </li>
                <li>
                  ※5: 必須、桁数、型など、データ構造から自明に決まる入力チェック
                </li>
              </ul>
              <p>
                赤い矢印は、手書きのコードから自動生成された処理を呼び出している箇所です。
              </p>
              <img src={boundaryImage} alt="自動生成コードと手動実装コードの境界を示す画面" />
            </div>
          </div>

          <div className={styles.imageTextStack} style={{ marginTop: '1rem' }}>
            <div className={styles.copyCard}>
              <Heading as="h3">4. UIは案件に合わせて作り込む</Heading>
              <p>
                自動生成はあくまで下支えです。検索条件や検索結果を画面上でどう見せるかは、
                通常のTypeScript開発でエンドユーザーや案件に合わせて自由に決められます。
                次の画像は、Nijoで構築した販売管理システムの例です。
              </p>
              <img src={businessUiImage} alt="Nijoで構築された販売管理システムの画面" />
            </div>
          </div>
        </section>

        <section className={styles.sectionBlockDark}>
          <div className={styles.sectionHeadingNarrow}>
            <p className={styles.sectionLabel}>Why Nijo</p>
            <Heading as="h2">生成の効率と、スクラッチの自由を両立する</Heading>
            <p>
              つなぎの部分を生成で受け持ちながら、業務の価値が出る部分はスクラッチ開発と同じ自由度で書ける。
              Nijoの設計は、この両立のためにあります。
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
            <Heading as="h2">採用前に知っておいてほしいこと</Heading>
            <p>
              Nijoには前提となる技術スタックがあり、カバーしない領域もあります。
              プロジェクトに合うかどうかを、ここで先に確認してください。
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
            <Heading as="h3">もっと詳しく</Heading>
            <p>
              ドキュメントでは、プロジェクト管理者、アーキテクト、プログラマそれぞれの視点からNijoを説明しています。
            </p>
            <Link className="button button--primary button--lg" to="/docs/intro">
              ドキュメントを読む
            </Link>
          </div>
        </section>
      </main>
    </Layout>
  );
}

export default Home;
