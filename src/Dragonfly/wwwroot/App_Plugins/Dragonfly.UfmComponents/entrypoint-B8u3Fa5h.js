import { UMB_AUTH_CONTEXT as M } from "@umbraco-cms/backoffice/auth";
import { umbHttpClient as G } from "@umbraco-cms/backoffice/http-client";
const Q = (e) => ({
  ...e,
  ...G.getConfig()
}), X = {
  bodySerializer: (e) => JSON.stringify(e, (t, r) => typeof r == "bigint" ? r.toString() : r)
};
function K({
  onRequest: e,
  onSseError: t,
  onSseEvent: r,
  responseTransformer: n,
  responseValidator: i,
  sseDefaultRetryDelay: c,
  sseMaxRetryAttempts: a,
  sseMaxRetryDelay: s,
  sseSleepFn: o,
  url: m,
  ...f
}) {
  let l;
  const g = o ?? ((d) => new Promise((h) => setTimeout(h, d)));
  return { stream: async function* () {
    let d = c ?? 3e3, h = 0;
    const w = f.signal ?? new AbortController().signal;
    for (; !w.aborted; ) {
      h++;
      const C = f.headers instanceof Headers ? f.headers : new Headers(f.headers);
      l !== void 0 && C.set("Last-Event-ID", l);
      try {
        const j = {
          redirect: "follow",
          ...f,
          body: f.serializedBody,
          headers: C,
          signal: w
        };
        let E = new Request(m, j);
        e && (E = await e(m, j));
        const y = await (f.fetch ?? globalThis.fetch)(E);
        if (!y.ok) throw new Error(`SSE failed: ${y.status} ${y.statusText}`);
        if (!y.body) throw new Error("No body in SSE response");
        const S = y.body.pipeThrough(new TextDecoderStream()).getReader();
        let b = "";
        const k = () => {
          try {
            S.cancel();
          } catch {
          }
        };
        w.addEventListener("abort", k);
        try {
          for (; ; ) {
            const { done: V, value: L } = await S.read();
            if (V) break;
            b += L, b = b.replace(/\r\n?/g, `
`);
            const $ = b.split(`

`);
            b = $.pop() ?? "";
            for (const J of $) {
              const F = J.split(`
`), O = [];
              let v;
              for (const x of F)
                if (x.startsWith("data:"))
                  O.push(x.replace(/^data:\s*/, ""));
                else if (x.startsWith("event:"))
                  v = x.replace(/^event:\s*/, "");
                else if (x.startsWith("id:"))
                  l = x.replace(/^id:\s*/, "");
                else if (x.startsWith("retry:")) {
                  const U = Number.parseInt(x.replace(/^retry:\s*/, ""), 10);
                  Number.isNaN(U) || (d = U);
                }
              let z, I = !1;
              if (O.length) {
                const x = O.join(`
`);
                try {
                  z = JSON.parse(x), I = !0;
                } catch {
                  z = x;
                }
              }
              I && (i && await i(z), n && (z = await n(z))), r?.({
                data: z,
                event: v,
                id: l,
                retry: d
              }), O.length && (yield z);
            }
          }
        } finally {
          w.removeEventListener("abort", k), S.releaseLock();
        }
        break;
      } catch (j) {
        if (t?.(j), a !== void 0 && h >= a)
          break;
        const E = Math.min(d * 2 ** (h - 1), s ?? 3e4);
        await g(E);
      }
    }
  }() };
}
const Y = (e) => {
  switch (e) {
    case "label":
      return ".";
    case "matrix":
      return ";";
    case "simple":
      return ",";
    default:
      return "&";
  }
}, Z = (e) => {
  switch (e) {
    case "form":
      return ",";
    case "pipeDelimited":
      return "|";
    case "spaceDelimited":
      return "%20";
    default:
      return ",";
  }
}, ee = (e) => {
  switch (e) {
    case "label":
      return ".";
    case "matrix":
      return ";";
    case "simple":
      return ",";
    default:
      return "&";
  }
}, P = ({
  allowReserved: e,
  explode: t,
  name: r,
  style: n,
  value: i
}) => {
  if (!t) {
    const s = (e ? i : i.map((o) => encodeURIComponent(o))).join(Z(n));
    switch (n) {
      case "label":
        return `.${s}`;
      case "matrix":
        return `;${r}=${s}`;
      case "simple":
        return s;
      default:
        return `${r}=${s}`;
    }
  }
  const c = Y(n), a = i.map((s) => n === "label" || n === "simple" ? e ? s : encodeURIComponent(s) : T({
    allowReserved: e,
    name: r,
    value: s
  })).join(c);
  return n === "label" || n === "matrix" ? c + a : a;
}, T = ({
  allowReserved: e,
  name: t,
  value: r
}) => {
  if (r == null)
    return "";
  if (typeof r == "object")
    throw new Error(
      "Deeply-nested arrays/objects aren’t supported. Provide your own `querySerializer()` to handle these."
    );
  return `${t}=${e ? r : encodeURIComponent(r)}`;
}, D = ({
  allowReserved: e,
  explode: t,
  name: r,
  style: n,
  value: i,
  valueOnly: c
}) => {
  if (i instanceof Date)
    return c ? i.toISOString() : `${r}=${i.toISOString()}`;
  if (n !== "deepObject" && !t) {
    let o = [];
    Object.entries(i).forEach(([f, l]) => {
      o = [...o, f, e ? l : encodeURIComponent(l)];
    });
    const m = o.join(",");
    switch (n) {
      case "form":
        return `${r}=${m}`;
      case "label":
        return `.${m}`;
      case "matrix":
        return `;${r}=${m}`;
      default:
        return m;
    }
  }
  const a = ee(n), s = Object.entries(i).map(
    ([o, m]) => T({
      allowReserved: e,
      name: n === "deepObject" ? `${r}[${o}]` : o,
      value: m
    })
  ).join(a);
  return n === "label" || n === "matrix" ? a + s : s;
}, te = /\{[^{}]+\}/g, re = ({ path: e, url: t }) => {
  let r = t;
  const n = t.match(te);
  if (n)
    for (const i of n) {
      let c = !1, a = i.substring(1, i.length - 1), s = "simple";
      a.endsWith("*") && (c = !0, a = a.substring(0, a.length - 1)), a.startsWith(".") ? (a = a.substring(1), s = "label") : a.startsWith(";") && (a = a.substring(1), s = "matrix");
      const o = e[a];
      if (o == null)
        continue;
      if (Array.isArray(o)) {
        r = r.replace(i, P({ explode: c, name: a, style: s, value: o }));
        continue;
      }
      if (typeof o == "object") {
        r = r.replace(
          i,
          D({
            explode: c,
            name: a,
            style: s,
            value: o,
            valueOnly: !0
          })
        );
        continue;
      }
      if (s === "matrix") {
        r = r.replace(
          i,
          `;${T({
            name: a,
            value: o
          })}`
        );
        continue;
      }
      const m = encodeURIComponent(
        s === "label" ? `.${o}` : o
      );
      r = r.replace(i, m);
    }
  return r;
}, se = ({
  baseUrl: e,
  path: t,
  query: r,
  querySerializer: n,
  url: i
}) => {
  const c = i.startsWith("/") ? i : `/${i}`;
  let a = (e ?? "") + c;
  t && (a = re({ path: t, url: a }));
  let s = r ? n(r) : "";
  return s.startsWith("?") && (s = s.substring(1)), s && (a += `?${s}`), a;
};
function B(e) {
  const t = e.body !== void 0;
  if (t && e.bodySerializer)
    return "serializedBody" in e ? e.serializedBody !== void 0 && e.serializedBody !== "" ? e.serializedBody : null : e.body !== "" ? e.body : null;
  if (t)
    return e.body;
}
const ne = async (e, t) => {
  const r = typeof t == "function" ? await t(e) : t;
  if (r)
    return e.scheme === "bearer" ? `Bearer ${r}` : e.scheme === "basic" ? `Basic ${btoa(r)}` : r;
}, H = ({
  parameters: e = {},
  ...t
} = {}) => (n) => {
  const i = [];
  if (n && typeof n == "object")
    for (const c in n) {
      const a = n[c];
      if (a == null)
        continue;
      const s = e[c] || t;
      if (Array.isArray(a)) {
        const o = P({
          allowReserved: s.allowReserved,
          explode: !0,
          name: c,
          style: "form",
          value: a,
          ...s.array
        });
        o && i.push(o);
      } else if (typeof a == "object") {
        const o = D({
          allowReserved: s.allowReserved,
          explode: !0,
          name: c,
          style: "deepObject",
          value: a,
          ...s.object
        });
        o && i.push(o);
      } else {
        const o = T({
          allowReserved: s.allowReserved,
          name: c,
          value: a
        });
        o && i.push(o);
      }
    }
  return i.join("&");
}, ae = (e) => {
  if (!e)
    return "stream";
  const t = e.split(";")[0]?.trim();
  if (t) {
    if (t.startsWith("application/json") || t.endsWith("+json"))
      return "json";
    if (t === "multipart/form-data")
      return "formData";
    if (["application/", "audio/", "image/", "video/"].some((r) => t.startsWith(r)))
      return "blob";
    if (t.startsWith("text/"))
      return "text";
  }
}, oe = (e, t) => t ? !!(e.headers.has(t) || e.query?.[t] || e.headers.get("Cookie")?.includes(`${t}=`)) : !1;
async function ie(e) {
  for (const t of e.security ?? []) {
    if (oe(e, t.name))
      continue;
    const r = await ne(t, e.auth);
    if (!r)
      continue;
    const n = t.name ?? "Authorization";
    switch (t.in) {
      case "query":
        e.query || (e.query = {}), e.query[n] = r;
        break;
      case "cookie":
        e.headers.append("Cookie", `${n}=${r}`);
        break;
      default:
        e.headers.set(n, r);
        break;
    }
  }
}
const N = (e) => se({
  baseUrl: e.baseUrl,
  path: e.path,
  query: e.query,
  querySerializer: typeof e.querySerializer == "function" ? e.querySerializer : H(e.querySerializer),
  url: e.url
}), R = (e, t) => {
  const r = { ...e, ...t };
  return r.baseUrl?.endsWith("/") && (r.baseUrl = r.baseUrl.substring(0, r.baseUrl.length - 1)), r.headers = W(e.headers, t.headers), r;
}, ce = (e) => {
  const t = [];
  return e.forEach((r, n) => {
    t.push([n, r]);
  }), t;
}, W = (...e) => {
  const t = new Headers();
  for (const r of e) {
    if (!r)
      continue;
    const n = r instanceof Headers ? ce(r) : Object.entries(r);
    for (const [i, c] of n)
      if (c === null)
        t.delete(i);
      else if (Array.isArray(c))
        for (const a of c)
          t.append(i, a);
      else c !== void 0 && t.set(
        i,
        typeof c == "object" ? JSON.stringify(c) : c
      );
  }
  return t;
};
class q {
  constructor() {
    this.fns = [];
  }
  clear() {
    this.fns = [];
  }
  eject(t) {
    const r = this.getInterceptorIndex(t);
    this.fns[r] && (this.fns[r] = null);
  }
  exists(t) {
    const r = this.getInterceptorIndex(t);
    return !!this.fns[r];
  }
  getInterceptorIndex(t) {
    return typeof t == "number" ? this.fns[t] ? t : -1 : this.fns.indexOf(t);
  }
  update(t, r) {
    const n = this.getInterceptorIndex(t);
    return this.fns[n] ? (this.fns[n] = r, t) : !1;
  }
  use(t) {
    return this.fns.push(t), this.fns.length - 1;
  }
}
const le = () => ({
  error: new q(),
  request: new q(),
  response: new q()
}), fe = H({
  allowReserved: !1,
  array: {
    explode: !0,
    style: "form"
  },
  object: {
    explode: !0,
    style: "deepObject"
  }
}), de = {
  "Content-Type": "application/json"
}, _ = (e = {}) => ({
  ...X,
  headers: de,
  parseAs: "auto",
  querySerializer: fe,
  ...e
}), ue = (e = {}) => {
  let t = R(_(), e);
  const r = () => ({ ...t }), n = (f) => (t = R(t, f), r()), i = le(), c = async (f) => {
    const l = {
      ...t,
      ...f,
      fetch: f.fetch ?? t.fetch ?? globalThis.fetch,
      headers: W(t.headers, f.headers),
      serializedBody: void 0
    };
    l.security && await ie(l), l.requestValidator && await l.requestValidator(l), l.body !== void 0 && l.bodySerializer && (l.serializedBody = l.bodySerializer(l.body)), (l.body === void 0 || l.serializedBody === "") && l.headers.delete("Content-Type");
    const g = l, p = N(g);
    return { opts: g, url: p };
  }, a = async (f) => {
    const l = f.throwOnError ?? t.throwOnError, g = f.responseStyle ?? t.responseStyle;
    let p, u;
    try {
      const { opts: d, url: h } = await c(f), w = {
        redirect: "follow",
        ...d,
        body: B(d)
      };
      p = new Request(h, w);
      for (const y of i.request.fns)
        y && (p = await y(p, d));
      const C = d.fetch;
      u = await C(p);
      for (const y of i.response.fns)
        y && (u = await y(u, p, d));
      const j = {
        request: p,
        response: u
      };
      if (u.ok) {
        const y = (d.parseAs === "auto" ? ae(u.headers.get("Content-Type")) : d.parseAs) ?? "json";
        if (u.status === 204 || u.headers.get("Content-Length") === "0") {
          let b;
          switch (y) {
            case "arrayBuffer":
            case "blob":
            case "text":
              b = await u[y]();
              break;
            case "formData":
              b = new FormData();
              break;
            case "stream":
              b = u.body;
              break;
            default:
              b = {};
              break;
          }
          return d.responseStyle === "data" ? b : {
            data: b,
            ...j
          };
        }
        let S;
        switch (y) {
          case "arrayBuffer":
          case "blob":
          case "formData":
          case "text":
            S = await u[y]();
            break;
          case "json": {
            const b = await u.text();
            S = b ? JSON.parse(b) : {};
            break;
          }
          case "stream":
            return d.responseStyle === "data" ? u.body : {
              data: u.body,
              ...j
            };
        }
        return y === "json" && (d.responseValidator && await d.responseValidator(S), d.responseTransformer && (S = await d.responseTransformer(S))), d.responseStyle === "data" ? S : {
          data: S,
          ...j
        };
      }
      const E = await u.text();
      let A;
      try {
        A = JSON.parse(E);
      } catch {
      }
      throw A ?? E;
    } catch (d) {
      let h = d;
      for (const w of i.error.fns)
        w && (h = await w(h, u, p, f));
      if (h = h || {}, l)
        throw h;
      return g === "data" ? void 0 : {
        error: h,
        request: p,
        response: u
      };
    }
  }, s = (f) => (l) => a({ ...l, method: f }), o = (f) => async (l) => {
    const { opts: g, url: p } = await c(l);
    return K({
      ...g,
      body: g.body,
      method: f,
      onRequest: async (u, d) => {
        let h = new Request(u, d);
        for (const w of i.request.fns)
          w && (h = await w(h, g));
        return h;
      },
      serializedBody: B(g),
      url: p
    });
  };
  return {
    buildUrl: (f) => N({ ...t, ...f }),
    connect: s("CONNECT"),
    delete: s("DELETE"),
    get: s("GET"),
    getConfig: r,
    head: s("HEAD"),
    interceptors: i,
    options: s("OPTIONS"),
    patch: s("PATCH"),
    post: s("POST"),
    put: s("PUT"),
    request: a,
    setConfig: n,
    sse: {
      connect: o("CONNECT"),
      delete: o("DELETE"),
      get: o("GET"),
      head: o("HEAD"),
      options: o("OPTIONS"),
      patch: o("PATCH"),
      post: o("POST"),
      put: o("PUT"),
      trace: o("TRACE")
    },
    trace: s("TRACE")
  };
}, he = ue(Q(_({ baseUrl: "https://localhost:44365" }))), pe = async (e, t) => {
  const r = await e.getContext(M);
  if (!r) {
    console.warn("UMB_AUTH_CONTEXT not available — extension API client will not be authenticated");
    return;
  }
  r.configureClient(he), console.log("Hello from my extension 🎉");
}, me = (e, t) => {
  console.log("Goodbye from my extension 👋");
};
export {
  pe as onInit,
  me as onUnload
};
//# sourceMappingURL=entrypoint-B8u3Fa5h.js.map
