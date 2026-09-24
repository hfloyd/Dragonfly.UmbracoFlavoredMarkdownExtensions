const n = /^umb:\/\/(document|media)\/([0-9a-fA-F]{32})$/;
function c(t) {
  if (!t) return [];
  let e = t;
  if (typeof e == "string")
    try {
      e = JSON.parse(e);
    } catch {
      return [];
    }
  return (Array.isArray(e) ? e : [e]).filter((r) => !!r && typeof r == "object").map(u);
}
function o(t, e) {
  return `${e ?? ""}${t.queryString ?? ""}` || void 0;
}
function u(t) {
  if (t.unique || !t.udi) return t;
  const e = n.exec(t.udi);
  if (!e) return t;
  const [, i, r] = e;
  return { ...t, type: t.type ?? i, unique: s(r) };
}
function s(t) {
  return [t.slice(0, 8), t.slice(8, 12), t.slice(12, 16), t.slice(16, 20), t.slice(20)].join("-").toLowerCase();
}
export {
  c as p,
  o as w
};
//# sourceMappingURL=link-value.function-BBn2Bd7X.js.map
