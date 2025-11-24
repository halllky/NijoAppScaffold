import type { ReactNode } from 'react';
import clsx from 'clsx';
import Heading from '@theme/Heading';
import styles from './styles.module.css';

type FeatureItem = {
  title: string;
  // Svg: React.ComponentType<React.ComponentProps<'svg'>>;
  description: ReactNode;
};

const FeatureList: FeatureItem[] = [
  {
    title: '可視化による合意形成',
    // Svg: require('@site/static/img/undraw_docusaurus_mountain.svg').default,
    description: (
      <>
        専用GUIエディタでデータ構造をグラフ化。
        エンジニアだけでなく、ドメインエキスパートやPMも交えて、
        システムの挙動を視覚的に共有・合意できます。
      </>
    ),
  },
  {
    title: '集約指向モデリング',
    // Svg: require('@site/static/img/undraw_docusaurus_tree.svg').default,
    description: (
      <>
        「集約(Aggregate)」という単位でデータを捉え、整合性を保証。
        データモデル、クエリモデル、コマンドモデルを定義し、
        堅牢なアプリケーションの土台を構築します。
      </>
    ),
  },
  {
    title: 'フルスタックかつ型安全',
    // Svg: require('@site/static/img/undraw_docusaurus_react.svg').default,
    description: (
      <>
        DB(Entity Framework Core)、バックエンド(ASP.NET Core)、
        フロントエンド(TypeScript)まで、型定義を一気通貫で自動生成。
        変更に強く、保守性の高いコードベースを提供します。
      </>
    ),
  },
];

function Feature({ title, description }: FeatureItem) {
  return (
    <div className={clsx('col col--4')}>
      {/* <div className="text--center">
        <Svg className={styles.featureSvg} role="img" />
      </div> */}
      <div className="text--center padding-horiz--md">
        <Heading as="h3">{title}</Heading>
        <p>{description}</p>
      </div>
    </div>
  );
}

export default function HomepageFeatures(): ReactNode {
  return (
    <section className={styles.features}>
      <div className="container">
        <div className="row">
          {FeatureList.map((props, idx) => (
            <Feature key={idx} {...props} />
          ))}
        </div>
      </div>
    </section>
  );
}
