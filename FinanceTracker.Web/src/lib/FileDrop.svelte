<script lang="ts">
  import { createEventDispatcher } from "svelte";

  export let accept = ".csv,text/csv";
  export let disabled = false;
  export let file: File | null = null;

  const dispatch = createEventDispatcher<{ change: File | null }>();
  let fileInput: HTMLInputElement;

  function setFile(f: File | null) {
    file = f;
    dispatch("change", f);
  }

  function onInput(e: Event) {
    const f = (e.currentTarget as HTMLInputElement).files?.[0] ?? null;
    setFile(f);
  }

  function onDrop(e: DragEvent) {
    e.preventDefault();
    if (disabled) return;
    const f = e.dataTransfer?.files?.[0] ?? null;
    if (f && (f.type.includes("csv") || f.name.endsWith(".csv"))) {
      setFile(f);
    }
  }

</script>

<label
  class={`group relative block rounded-2xl border border-white/10 bg-white/[0.05] backdrop-blur-xl p-5
          transition-all duration-200 hover:border-white/20 hover:scale-[1.01] active:scale-[0.99]
          ${disabled ? "opacity-60 cursor-not-allowed" : "cursor-pointer"}`}
  on:dragover|preventDefault
  on:drop={onDrop}
>
  <input bind:this={fileInput} class="hidden" type="file" accept={accept} disabled={disabled} on:change={onInput} />

  <div class="flex items-start gap-4">
    <div class="grid h-11 w-11 place-items-center rounded-xl bg-white/10 ring-1 ring-white/10">
      <svg width="18" height="18" viewBox="0 0 24 24" fill="none" class="opacity-90">
        <path d="M12 3v10" stroke="currentColor" stroke-width="2" stroke-linecap="round"/>
        <path d="M8 7l4-4 4 4" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"/>
        <path d="M4 14v4a3 3 0 0 0 3 3h10a3 3 0 0 0 3-3v-4" stroke="currentColor" stroke-width="2" stroke-linecap="round"/>
      </svg>
    </div>

    <div class="min-w-0">
      <p class="text-sm font-semibold text-zinc-100">Drop your CSV here</p>
      <p class="mt-1 text-xs text-zinc-400">or click to browse • .csv</p>

      {#if file}
        <p class="mt-3 truncate text-xs text-zinc-200 animate-in fade-in slide-in-from-bottom-2 duration-300">
          <span class="inline-flex items-center gap-2 rounded-lg bg-emerald-500/20 px-2 py-1 ring-1 ring-emerald-500/30">
            <svg class="h-3 w-3 text-emerald-400" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" />
            </svg>
            {file.name}
          </span>
        </p>
      {/if}
    </div>
  </div>

  <div
    class="pointer-events-none absolute inset-0 rounded-2xl opacity-0 transition group-hover:opacity-100
           bg-[radial-gradient(700px_circle_at_0%_0%,rgba(99,102,241,0.18),transparent_55%),
              radial-gradient(800px_circle_at_100%_0%,rgba(236,72,153,0.14),transparent_55%)]"
  ></div>
</label>
