import React, { createContext, useContext, useState, useRef, useEffect } from 'react';

// ----------------------------------------------------------------------
// 1. Context Definition
// ----------------------------------------------------------------------
interface DebugContextType {
  count: number;
  increment: () => void;
  text: string;
  setText: (s: string) => void;
}

const DebugContext = createContext<DebugContextType | undefined>(undefined);

// ----------------------------------------------------------------------
// 2. Components
// ----------------------------------------------------------------------

/**
 * Context Provider
 * Has state inside.
 */
const DebugProvider = ({ children }: { children: React.ReactNode }) => {
  const [count, setCount] = useState(0);
  const [text, setText] = useState('hello');

  const increment = () => setCount((c) => c + 1);

  // Note: Value object is recreated on every render unless memoized.
  // For strict testing of consumers, this is often the case to watch out for.
  const value = { count, increment, text, setText };

  const renderCountRef = useRef(0);
  renderCountRef.current++;

  return (
    <DebugContext.Provider value={value}>
      <div className="border border-blue-500 p-4 m-2 rounded bg-blue-50">
        <h3 className="font-bold text-blue-700">Provider Component (State Holder)</h3>
        <div className="text-sm my-2">
          <p>Render Count: <span className="font-mono bg-white px-1 ml-1">{renderCountRef.current}</span></p>
          <div className="flex gap-2 mt-1">
            <button onClick={increment} className="bg-blue-600 text-white px-2 py-1 rounded text-xs hover:bg-blue-700">
              Increment Count
            </button>
            <input
              value={text}
              onChange={e => setText(e.target.value)}
              className="border border-gray-300 px-1 text-xs"
            />
          </div>
          <p className="text-gray-500 text-xs mt-1">State: count={count}, text="{text}"</p>
        </div>

        <div className="pl-4 border-l-2 border-blue-200 mt-4">
          <p className="text-xs text-gray-400 mb-2">Children prop rendered below:</p>
          {children}
        </div>
      </div>
    </DebugContext.Provider>
  );
};

/**
 * Component that uses context
 */
const ContextConsumer = () => {
  const context = useContext(DebugContext);
  const renderCountRef = useRef(0);
  renderCountRef.current++;

  if (!context) return <div className="text-red-500">No Context</div>;

  return (
    <div className="border border-green-500 p-4 m-2 rounded bg-green-50">
      <h4 className="font-bold text-green-700">Context Consumer</h4>
      <p className="text-xs text-gray-600 mb-2">Constantly consumes context via useContext hook.</p>

      <p>Render Count: <span className="font-mono bg-white px-1 ml-1 font-bold text-red-600">{renderCountRef.current}</span></p>
      <p>Count from Context: {context.count}</p>

      <ContextConsumerChild />
    </div>
  );
};

const ContextConsumerChild = () => {
  const renderCountRef = useRef(0);
  renderCountRef.current++;

  return (
    <div className="border border-green-500 p-4 m-2 rounded bg-green-50">
      <h4 className="font-bold text-green-700">Context Consumer Child</h4>
      <p className="text-xs text-gray-600 mb-2">Child of Context Consumer.</p>

      <p>Render Count: <span className="font-mono bg-white px-1 ml-1 font-bold text-red-600">{renderCountRef.current}</span></p>
    </div>
  );
};

/**
 * Component that does NOT use context
 */
const NonConsumer = () => {
  const renderCountRef = useRef(0);
  renderCountRef.current++;

  return (
    <div className="border border-gray-400 p-4 m-2 rounded bg-gray-50">
      <h4 className="font-bold text-gray-700">Non-Consumer</h4>
      <p className="text-xs text-gray-600 mb-2">Does not use useContext.</p>

      <p>Render Count: <span className="font-mono bg-white px-1 ml-1 font-bold">{renderCountRef.current}</span></p>
      <p className="text-xs mt-2 text-gray-500">
        If this component is passed as 'children' to the Provider from a parent that didn't re-render,
        this component will NOT re-render even if the Provider updates (unless the Provider forces a re-render of children by cloning etc, which is rare).
      </p>

      <NonConsumerChild />
    </div>
  );
};

const NonConsumerChild = () => {
  const renderCountRef = useRef(0);
  renderCountRef.current++;

  return (
    <div className="border border-gray-400 p-4 m-2 rounded bg-gray-50">
      <h4 className="font-bold text-gray-700">Non-Consumer Child</h4>
      <p className="text-xs text-gray-600 mb-2">Child of Non-Consumer.</p>

      <p>Render Count: <span className="font-mono bg-white px-1 ml-1 font-bold">{renderCountRef.current}</span></p>
      <p className="text-xs mt-2 text-gray-500">
        This component is a child of Non-Consumer. It also does not use context.
      </p>
    </div>
  );
}

/**
 * Component that uses context, but is optimized with memo?
 * (Memo doesn't help if context changes, useContext triggers re-render anyway)
 */
const MemoizedConsumer = React.memo(() => {
  const context = useContext(DebugContext);
  const renderCountRef = useRef(0);
  renderCountRef.current++;

  return (
    <div className="border border-orange-500 p-4 m-2 rounded bg-orange-50">
      <h4 className="font-bold text-orange-700">Memoized Consumer</h4>
      <p className="text-xs text-gray-600 mb-2">Wrapped in React.memo, but uses useContext.</p>

      <p>Render Count: <span className="font-mono bg-white px-1 ml-1 font-bold text-red-600">{renderCountRef.current}</span></p>
      <p className="text-xs mt-1">
        React.memo cannot prevent re-renders triggered by context updates.
      </p>
    </div>
  );
});

// ----------------------------------------------------------------------
// 3. Page Component
// ----------------------------------------------------------------------

export default function ReactContextDebugging() {
  const renderCountRef = useRef(0);
  renderCountRef.current++;

  return (
    <div className="p-4 w-full h-full overflow-y-auto">
      <h2 className="text-2xl font-bold mb-4">React Context Re-render Debugging</h2>

      <div className="mb-6 p-4 border border-purple-300 bg-purple-50 rounded">
        <h3 className="font-bold text-purple-800">Page Root</h3>
        <p>Render Count: {renderCountRef.current}</p>
        <p className="text-sm text-gray-600">The Provider is rendered below. Components are passed as children to the provider.</p>
      </div>

      <div className="flex flex-col gap-6">

        {/* Scenario A: Components as Children */}
        <section>
          <h3 className="text-xl font-bold border-b mb-2">Scenario A: Composition (Passing children)</h3>
          <p className="text-sm mb-2">
            The Consumer and NonConsumer are passed as `children` to `DebugProvider`.
            Since `ReactContextDebugging` (Page Root) does not re-render when Provider state changes,
            the {"`{children}`"} prop passed to Provider remains referentially equal.
            React skips re-rendering `NonConsumer`. `ContextConsumer` re-renders because it subscribes to context.
          </p>

          <DebugProvider>
            <div className="flex gap-2 flex-wrap items-start">
              <div className="w-64">
                <ContextConsumer />
              </div>
              <div className="w-64">
                <NonConsumer />
              </div>
              <div className="w-64">
                <MemoizedConsumer />
              </div>
            </div>
          </DebugProvider>
        </section>

        {/* Scenario B: Components defined inside Provider's render tree directly (Anti-pattern often responsible for perf issues) */}
        {/* We simulate this by defining a separate component that includes the provider and children internally */}
      </div>
    </div>
  );
}
