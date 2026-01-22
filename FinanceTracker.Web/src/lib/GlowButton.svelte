<script lang="ts">
  export let type: "button" | "submit" = "button";
  export let disabled = false;
  export let className = "";
</script>

<button
  type={type}
  disabled={disabled}
  class={`group relative inline-flex items-center justify-center overflow-hidden rounded-xl px-4 py-2
          text-sm font-semibold text-zinc-900 disabled:cursor-not-allowed disabled:opacity-50
          transition-all duration-200 hover:scale-[1.02] active:scale-[0.98] ${className}`}
  on:click
>
  <span class="absolute inset-0 bg-white transition-opacity duration-200"></span>

  <span
    class="absolute inset-0 opacity-0 transition-opacity duration-300 group-hover:opacity-100
           bg-[radial-gradient(160px_circle_at_20%_0%,rgba(99,102,241,0.45),transparent_55%),
              radial-gradient(180px_circle_at_80%_120%,rgba(236,72,153,0.35),transparent_60%)]"
  ></span>

  <span class="absolute inset-0 opacity-0 transition-opacity duration-150 group-active:opacity-100 bg-black/10"></span>

  <span class="relative flex items-center gap-2">
    <slot />
  </span>
</button>
