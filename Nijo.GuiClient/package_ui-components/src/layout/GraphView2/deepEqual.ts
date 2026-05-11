// 簡易的なDeep Equalの実装
export function deepEqual<T>(x: T, y: T): boolean {
  if (x === y) return true;

  if (typeof x === "object" && x != null && typeof y === "object" && y != null) {
    if (Array.isArray(x) || Array.isArray(y)) {
      if (!Array.isArray(x) || !Array.isArray(y) || x.length !== y.length) return false;
      for (let i = 0; i < x.length; i++) {
        if (!deepEqual(x[i], y[i])) return false;
      }
      return true;
    }

    const keysX = Object.keys(x);
    const keysY = Object.keys(y);

    if (keysX.length !== keysY.length) return false;

    for (const key of keysX) {
      const valueX = (x as Record<string, unknown>)[key];
      const valueY = (y as Record<string, unknown>)[key];
      if (!Object.prototype.hasOwnProperty.call(y, key) || !deepEqual(valueX, valueY)) {
        return false;
      }
    }

    return true;
  }

  return false;
}
