<script lang="ts">
  import "../app.css";
  import { page } from "$app/stores";

  const linkClass = (href: string) => {
    const active = $page.url.pathname === href;
    return `rounded-xl px-3 py-2 text-sm transition ${
      active
        ? "bg-white/10 text-white ring-1 ring-white/15"
        : "text-zinc-300 hover:bg-white/5 hover:text-white"
    }`;
  };
</script>

<div class="min-h-screen bg-zinc-950 text-zinc-100">
  <!-- background sauce -->
  <div class="pointer-events-none fixed inset-0 -z-10">
    <div
      class="absolute inset-0
             bg-[radial-gradient(1200px_circle_at_20%_-10%,rgba(99,102,241,0.25),transparent_55%),
                radial-gradient(1000px_circle_at_80%_0%,rgba(236,72,153,0.18),transparent_55%),
                radial-gradient(900px_circle_at_50%_110%,rgba(16,185,129,0.10),transparent_55%)]"
    ></div>
    <div
      class="absolute inset-0 opacity-[0.12]
             [background-image:radial-gradient(rgba(255,255,255,0.7)_1px,transparent_1px)]
             [background-size:26px_26px]"
    ></div>
  </div>

  <div class="mx-auto max-w-5xl px-4 py-8">
    <header class="mb-6 flex items-center justify-between">
      <div class="flex items-center gap-3">
        <div class="h-9 w-9 rounded-2xl bg-white/10 ring-1 ring-white/10"></div>
        <div>
          <div class="text-base font-semibold tracking-tight">FinanceTracker</div>
          <div class="text-xs text-zinc-400">local-first • clean • fast</div>
        </div>
      </div>

      <nav class="flex gap-2 rounded-2xl border border-white/10 bg-white/[0.05] p-1 backdrop-blur-xl">
        <a class={linkClass("/")} href="/">Dashboard</a>
        <a class={linkClass("/import")} href="/import">Import</a>
        <a class={linkClass("/docs")} href="/docs">Docs</a>
      </nav>
    </header>

    <main class="rounded-2xl border border-white/10 bg-white/[0.05] p-6 backdrop-blur-xl">
      <slot />
    </main>
  </div>
</div>
