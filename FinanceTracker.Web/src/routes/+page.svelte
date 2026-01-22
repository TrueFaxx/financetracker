<script lang="ts">
  import { onMount } from "svelte";
  import CardGlass from "$lib/CardGlass.svelte";
  import { formatCurrency, formatDate, formatMonth } from "$lib/utils";

  let monthly: any[] = [];
  let month = new Date().toISOString().slice(0, 7);
  let top: any[] = [];
  let biggest: any[] = [];
  let fraud: any[] = [];
  let fraudThreshold = 400;
  let loading = true;
  let err = "";

  async function loadAll() {
    loading = true;
    err = "";
    try {
      let monthlyRes: Response | null = null;
      let topRes: Response | null = null;
      let biggestRes: Response | null = null;
      let fraudRes: Response | null = null;

      try {
        [monthlyRes, topRes, biggestRes, fraudRes] = await Promise.all([
          fetch("/api/summary/monthly?months=6"),
          fetch(`/api/top/merchants?month=${month}`),
          fetch(`/api/biggest?month=${month}`),
          fetch(`/api/fraud?month=${month}`),
        ]);
      } catch (networkError: any) {
        throw new Error("API server is not running. Please start the API with: cd FinanceTracker.Api && dotnet run");
      }

      const errors: string[] = [];
      const isConnectionError = !monthlyRes || !topRes || !biggestRes || !fraudRes;
      
      if (!monthlyRes?.ok) {
        const errorMsg = monthlyRes?.status === 0 || !monthlyRes ? "Connection refused" : monthlyRes.statusText || "Failed";
        errors.push(`Monthly summary: ${errorMsg}`);
      }
      if (!topRes?.ok) {
        const errorMsg = topRes?.status === 0 || !topRes ? "Connection refused" : topRes.statusText || "Failed";
        errors.push(`Top merchants: ${errorMsg}`);
      }
      if (!biggestRes?.ok) {
        const errorMsg = biggestRes?.status === 0 || !biggestRes ? "Connection refused" : biggestRes.statusText || "Failed";
        errors.push(`Biggest spends: ${errorMsg}`);
      }
      if (!fraudRes?.ok) {
        const errorMsg = fraudRes?.status === 0 || !fraudRes ? "Connection refused" : fraudRes.statusText || "Failed";
        errors.push(`Fraud detection: ${errorMsg}`);
      }

      if (errors.length > 0) {
        if (isConnectionError || errors.some(e => e.includes("Connection refused"))) {
          throw new Error("API server is not running. Please start the API with: cd FinanceTracker.Api && dotnet run");
        }
        throw new Error(`Failed to load data: ${errors.join(", ")}`);
      }

      monthly = await monthlyRes.json();
      top = await topRes.json();
      biggest = await biggestRes.json();
      const fraudPayload = await fraudRes.json();
      fraudThreshold = fraudPayload?.threshold ?? 400;
      fraud = fraudPayload?.items ?? [];
    } catch (e: any) {
      err = e?.message ?? String(e);
      // Set empty arrays on error so UI doesn't break
      monthly = [];
      top = [];
      biggest = [];
      fraud = [];
      fraudThreshold = 400;
    } finally {
      loading = false;
    }
  }

  onMount(loadAll);

  async function onMonthChange(e: Event) {
    month = (e.currentTarget as HTMLInputElement).value;
    await loadAll();
  }

  function getCurrentMonthStats() {
    const [year, monthNum] = month.split("-").map(Number);
    return monthly.find((m) => m.year === year && m.month === monthNum);
  }

  const currentStats = getCurrentMonthStats();
</script>

<div class="space-y-8">
  <!-- Header with Month Selector -->
  <div class="flex flex-col gap-4 rounded-2xl border border-white/5 bg-gradient-to-br from-white/[0.04] via-white/[0.02] to-transparent p-6 md:flex-row md:items-center md:justify-between">
    <div>
      <div class="flex flex-wrap items-center gap-3">
        <h1 class="text-3xl font-bold tracking-tight">Dashboard</h1>
        {#if !loading && fraud.length > 0}
          <span class="rounded-full bg-amber-500/20 px-3 py-1 text-xs font-semibold text-amber-300 ring-1 ring-amber-500/30">
            {fraud.length} Fraud Alert{fraud.length !== 1 ? "s" : ""}
          </span>
        {/if}
      </div>
      <p class="mt-2 text-sm text-zinc-400">Track your finances at a glance and review any large purchases.</p>
    </div>
    <label class="flex items-center gap-2 rounded-xl border border-white/10 bg-white/[0.05] px-4 py-2 backdrop-blur-xl">
      <span class="text-sm text-zinc-300">Month:</span>
      <input
        type="month"
        bind:value={month}
        on:change={onMonthChange}
        class="bg-transparent text-sm text-zinc-100 outline-none"
      />
    </label>
  </div>

  <!-- Error State -->
  {#if err}
    <CardGlass className="border-amber-500/40 bg-gradient-to-br from-amber-500/10 to-rose-500/5">
      <div class="space-y-3">
        <div class="flex items-start gap-3">
          <svg class="h-5 w-5 text-amber-400 mt-0.5 flex-shrink-0" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z" />
          </svg>
          <div class="flex-1">
            <p class="text-amber-200 font-semibold">Connection Error</p>
            <p class="mt-1 text-sm text-amber-300/80">{err}</p>
            {#if err.includes("API server is not running")}
              <div class="mt-3 rounded-lg bg-amber-500/10 border border-amber-500/20 p-3">
                <p class="text-xs font-medium text-amber-300 mb-2">To start the API server:</p>
                <code class="block text-xs text-amber-200 bg-black/20 px-3 py-2 rounded border border-amber-500/20">
                  cd FinanceTracker.Api<br />
                  dotnet run
                </code>
                <p class="text-xs text-amber-400/70 mt-2">The API should start on http://localhost:5043</p>
              </div>
            {/if}
          </div>
        </div>
      </div>
    </CardGlass>
  {/if}

  <!-- Loading State -->
  {#if loading}
    <div class="grid gap-6 md:grid-cols-3">
      {#each Array(3) as _}
        <CardGlass>
          <div class="space-y-3">
            <div class="h-4 w-24 animate-pulse rounded bg-white/10"></div>
            <div class="h-8 w-32 animate-pulse rounded bg-white/10"></div>
          </div>
        </CardGlass>
      {/each}
    </div>
  {:else}
    <!-- Fraud Alert Section -->
    {#if fraud.length > 0}
      <CardGlass className="border-amber-500/40 bg-gradient-to-br from-amber-500/15 via-amber-500/5 to-rose-500/10">
        <div class="space-y-4">
          <div class="flex flex-col gap-3 md:flex-row md:items-center md:justify-between">
            <div class="flex items-center gap-3">
              <div class="rounded-lg bg-amber-500/20 p-2 ring-1 ring-amber-500/30">
                <svg class="h-5 w-5 text-amber-400" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z" />
                </svg>
              </div>
              <div class="flex-1">
                <h2 class="text-lg font-semibold text-amber-200">⚠️ Possible Fraud: Large Purchases</h2>
                <p class="mt-1 text-sm text-amber-300/80">
                  {fraud.length} purchase{fraud.length !== 1 ? "s" : ""} over {formatCurrency(fraudThreshold)} found this month
                </p>
              </div>
            </div>
            <div class="flex items-center gap-2 rounded-full border border-amber-500/30 bg-amber-500/15 px-3 py-1 text-xs font-medium text-amber-200">
              Threshold: {formatCurrency(fraudThreshold)}
            </div>
          </div>
          <div class="grid gap-3 lg:grid-cols-2">
            {#each fraud as f}
              <div class="rounded-xl border border-amber-500/20 bg-amber-500/5 p-4 transition hover:bg-amber-500/10">
                <div class="flex items-start justify-between gap-3">
                  <div class="min-w-0 flex-1 space-y-1">
                    <div class="flex flex-wrap items-center gap-2">
                      <p class="text-sm font-semibold text-amber-200">{f.merchant || "Unknown"}</p>
                      <span class="rounded bg-amber-500/20 px-2 py-0.5 text-xs font-medium text-amber-300 ring-1 ring-amber-500/30">
                        Possible Fraud
                      </span>
                    </div>
                    <p class="text-xs text-amber-300/70 line-clamp-1">{f.description}</p>
                    <p class="text-xs text-amber-400/60">{formatDate(f.date)}</p>
                  </div>
                  <div class="text-right">
                    <p class="text-base font-bold text-amber-300 whitespace-nowrap">{formatCurrency(f.spent)}</p>
                    <p class="text-xs text-amber-300/70">Large purchase</p>
                  </div>
                </div>
              </div>
            {/each}
          </div>
        </div>
      </CardGlass>
    {/if}

    <!-- Current Month Stats -->
    {#if currentStats}
      <div class="grid gap-6 md:grid-cols-3">
        <CardGlass className="border-emerald-400/25">
          <div class="space-y-1">
            <p class="text-xs font-medium text-zinc-400 uppercase tracking-wide">Income</p>
            <p class="text-2xl font-bold text-emerald-300">{formatCurrency(currentStats.income)}</p>
          </div>
        </CardGlass>
        <CardGlass className="border-rose-400/25">
          <div class="space-y-1">
            <p class="text-xs font-medium text-zinc-400 uppercase tracking-wide">Expenses</p>
            <p class="text-2xl font-bold text-rose-300">{formatCurrency(currentStats.expense)}</p>
          </div>
        </CardGlass>
        <CardGlass className={currentStats.net >= 0 ? "border-emerald-400/25" : "border-rose-400/25"}>
          <div class="space-y-1">
            <p class="text-xs font-medium text-zinc-400 uppercase tracking-wide">Net</p>
            <p class="text-2xl font-bold {currentStats.net >= 0 ? 'text-emerald-300' : 'text-rose-300'}">
              {formatCurrency(currentStats.net)}
            </p>
          </div>
        </CardGlass>
      </div>
    {/if}

    <!-- Monthly Overview -->
    <CardGlass>
      <div class="space-y-4">
        <div class="flex items-center justify-between">
          <h2 class="text-lg font-semibold">Last 6 Months</h2>
        </div>
        <div class="space-y-2">
          {#each monthly.slice().reverse() as m}
            <div class="flex items-center justify-between rounded-lg border border-white/5 bg-white/[0.02] p-3 transition hover:bg-white/[0.05]">
              <div class="flex items-center gap-3">
                <div class="h-10 w-10 rounded-lg bg-white/5 flex items-center justify-center">
                  <span class="text-xs font-medium text-zinc-400">{String(m.month).padStart(2, "0")}</span>
                </div>
                <div>
                  <p class="text-sm font-medium text-zinc-100">{formatMonth(m.year, m.month)}</p>
                  <p class="text-xs text-zinc-400">{m.year}</p>
                </div>
              </div>
              <div class="flex items-center gap-4 text-sm">
                <div class="text-right">
                  <p class="text-xs text-zinc-400">Income</p>
                  <p class="font-medium text-emerald-300">{formatCurrency(m.income)}</p>
                </div>
                <div class="text-right">
                  <p class="text-xs text-zinc-400">Expense</p>
                  <p class="font-medium text-rose-300">{formatCurrency(m.expense)}</p>
                </div>
                <div class="text-right min-w-[80px]">
                  <p class="text-xs text-zinc-400">Net</p>
                  <p class="font-semibold {m.net >= 0 ? 'text-emerald-300' : 'text-rose-300'}">
                    {formatCurrency(m.net)}
                  </p>
                </div>
              </div>
            </div>
          {/each}
        </div>
      </div>
    </CardGlass>

    <!-- Top Merchants & Biggest Spends -->
    <div class="grid gap-6 md:grid-cols-2">
      <!-- Top Merchants -->
      <CardGlass>
        <div class="space-y-4">
          <h3 class="text-lg font-semibold">Top Merchants</h3>
          {#if top.length === 0}
            <p class="text-sm text-zinc-400">No merchant data for this month</p>
          {:else}
            <div class="space-y-2">
              {#each top as t, i}
                <div class="flex items-center justify-between rounded-lg border border-white/5 bg-white/[0.02] p-3 transition hover:bg-white/[0.05]">
                  <div class="flex items-center gap-3">
                    <div class="flex h-6 w-6 items-center justify-center rounded bg-white/10 text-xs font-semibold text-zinc-300">
                      {i + 1}
                    </div>
                    <p class="text-sm font-medium text-zinc-100">{t.merchant || "Unknown"}</p>
                  </div>
                  <p class="text-sm font-semibold text-rose-300">{formatCurrency(t.spent)}</p>
                </div>
              {/each}
            </div>
          {/if}
        </div>
      </CardGlass>

      <!-- Biggest Spends -->
      <CardGlass>
        <div class="space-y-4">
          <h3 class="text-lg font-semibold">Biggest Spends</h3>
          {#if biggest.length === 0}
            <p class="text-sm text-zinc-400">No spending data for this month</p>
          {:else}
            <div class="space-y-2">
              {#each biggest as b}
                {@const isFraud = b.spent >= fraudThreshold}
                <div class="rounded-lg border {isFraud ? 'border-amber-500/30 bg-amber-500/5' : 'border-white/5 bg-white/[0.02]'} p-3 transition hover:bg-white/[0.05]">
                  <div class="flex items-start justify-between gap-3">
                    <div class="min-w-0 flex-1">
                      <div class="flex items-center gap-2">
                        <p class="text-sm font-medium {isFraud ? 'text-amber-200' : 'text-zinc-100'}">{b.merchant || "Unknown"}</p>
                        {#if isFraud}
                          <span class="rounded bg-amber-500/20 px-1.5 py-0.5 text-xs font-medium text-amber-300 ring-1 ring-amber-500/30">
                            Possible Fraud
                          </span>
                        {/if}
                      </div>
                      <p class="mt-1 text-xs {isFraud ? 'text-amber-300/70' : 'text-zinc-400'} line-clamp-1">{b.description}</p>
                      <p class="mt-1 text-xs {isFraud ? 'text-amber-400/60' : 'text-zinc-500'}">{formatDate(b.date)}</p>
                    </div>
                    <p class="text-sm font-semibold {isFraud ? 'text-amber-300' : 'text-rose-300'} whitespace-nowrap">{formatCurrency(b.spent)}</p>
                  </div>
                </div>
              {/each}
            </div>
          {/if}
        </div>
      </CardGlass>
    </div>
  {/if}
</div>
