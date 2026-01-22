<script lang="ts">
  import { goto } from "$app/navigation";
  import { fly, fade, scale } from "svelte/transition";
  import CardGlass from "$lib/CardGlass.svelte";
  import GlowButton from "$lib/GlowButton.svelte";
  import FileDrop from "$lib/FileDrop.svelte";

  let file: File | null = null;
  let msg = "";
  let kind: "ok" | "err" | "" = "";
  let uploading = false;
  let importedCount = 0;
  let uploadProgress = 0;

  async function upload() {
    if (!file) {
      kind = "err";
      msg = "Please select a CSV file first.";
      return;
    }

    uploading = true;
    uploadProgress = 0;
    kind = "";
    msg = "";

    // Simulate progress animation
    const progressInterval = setInterval(() => {
      if (uploadProgress < 90) {
        uploadProgress += 10;
      }
    }, 100);

    try {
      const fd = new FormData();
      fd.append("file", file);

      const res = await fetch("/api/import/csv", { method: "POST", body: fd });

      clearInterval(progressInterval);
      uploadProgress = 100;

      if (!res.ok) {
        kind = "err";
        const errorText = await res.text();
        msg = errorText || "Failed to import file. Please check the file format.";
        uploading = false;
        uploadProgress = 0;
        return;
      }

      const data = await res.json();
      importedCount = data.imported || 0;
      kind = "ok";
      msg = `Successfully imported ${importedCount} transaction${importedCount !== 1 ? "s" : ""} ✅`;

      // Clear the file after successful upload
      setTimeout(() => {
        file = null;
      }, 500);

      // Auto-redirect to dashboard after 2 seconds
      setTimeout(() => {
        goto("/");
      }, 2000);
    } catch (e: any) {
      clearInterval(progressInterval);
      kind = "err";
      msg = e?.message ?? "An unexpected error occurred. Please try again.";
      uploadProgress = 0;
    } finally {
      uploading = false;
    }
  }

  function clearFile() {
    file = null;
    msg = "";
    kind = "";
    uploadProgress = 0;
  }
</script>

<div class="space-y-6">
  <div in:fade={{ duration: 300 }}>
    <h1 class="text-3xl font-bold tracking-tight">Import Transactions</h1>
    <p class="mt-2 text-sm text-zinc-400">
      Upload a CSV file with your transaction data. The system will automatically detect common bank formats.
    </p>
  </div>

  <div in:fly={{ y: 20, duration: 400, delay: 100 }}>
    <CardGlass>
      <div class="space-y-4">
        <div>
          <h2 class="text-sm font-semibold text-zinc-300 mb-2">Supported Format</h2>
          <p class="text-xs text-zinc-400 mb-3">
            Works best with headers like:
          </p>
          <div class="flex flex-wrap gap-2">
            {#each ["Date", "Description", "Amount"] as tag, i}
              <span
                class="rounded-lg bg-white/10 px-3 py-1.5 text-xs font-medium ring-1 ring-white/10
                       transition-all duration-200 hover:scale-105 hover:bg-white/15"
                in:scale={{ duration: 300, delay: 200 + i * 50 }}
              >
                {tag}
              </span>
            {/each}
          </div>
        </div>
      </div>
    </CardGlass>
  </div>

  <div in:fly={{ y: 20, duration: 400, delay: 200 }}>
    <FileDrop bind:file on:change={(e) => (file = e.detail)} disabled={uploading} />
  </div>

  <!-- Upload Progress Bar -->
  {#if uploading}
    <div in:fly={{ y: 10, duration: 300 }} class="space-y-2">
      <div class="h-2 w-full overflow-hidden rounded-full bg-white/5">
        <div
          class="h-full bg-gradient-to-r from-indigo-500 via-purple-500 to-pink-500 transition-all duration-300 ease-out"
          style="width: {uploadProgress}%"
        ></div>
      </div>
      <p class="text-xs text-center text-zinc-400">Uploading... {uploadProgress}%</p>
    </div>
  {/if}

  <div class="flex items-center justify-end gap-3" in:fly={{ y: 20, duration: 400, delay: 300 }}>
    {#if file && !uploading}
      <button
        on:click={clearFile}
        class="rounded-xl border border-white/10 bg-white/[0.05] px-4 py-2 text-sm font-semibold text-zinc-300
               transition-all duration-200 hover:bg-white/[0.08] hover:text-white hover:scale-105 active:scale-95"
        in:scale={{ duration: 200 }}
      >
        Clear
      </button>
    {/if}
    <GlowButton on:click={upload} disabled={!file || uploading}>
      {#if uploading}
        <svg class="h-4 w-4 animate-spin" fill="none" viewBox="0 0 24 24">
          <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
          <path
            class="opacity-75"
            fill="currentColor"
            d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"
          ></path>
        </svg>
        <span>Uploading…</span>
      {:else}
        <svg class="h-4 w-4 transition-transform duration-200 group-hover:translate-y-[-2px]" fill="none" viewBox="0 0 24 24" stroke="currentColor">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M7 16a4 4 0 01-.88-7.903A5 5 0 1115.9 6L16 6a5 5 0 011 9.9M15 13l-3-3m0 0l-3 3m3-3v12" />
        </svg>
        <span>Upload CSV</span>
      {/if}
    </GlowButton>
  </div>

  {#if msg}
    <div in:fly={{ y: 20, duration: 400 }} out:fade={{ duration: 200 }}>
      <CardGlass className={kind === "ok" ? "border-emerald-400/25" : "border-rose-400/25"}>
        <div class="flex items-start gap-3">
          {#if kind === "ok"}
            <svg
              class="h-5 w-5 text-emerald-400 mt-0.5 flex-shrink-0 animate-in spin-in duration-500"
              fill="none"
              viewBox="0 0 24 24"
              stroke="currentColor"
            >
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" />
            </svg>
          {:else}
            <svg
              class="h-5 w-5 text-rose-400 mt-0.5 flex-shrink-0 animate-in spin-in duration-500"
              fill="none"
              viewBox="0 0 24 24"
              stroke="currentColor"
            >
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
            </svg>
          {/if}
          <div class="flex-1">
            <p class={kind === "ok" ? "text-emerald-200" : "text-rose-200"}>{msg}</p>
            {#if kind === "ok"}
              <p class="mt-2 text-xs text-emerald-300/70 animate-pulse">Redirecting to dashboard...</p>
            {/if}
          </div>
        </div>
      </CardGlass>
    </div>
  {/if}
</div>
