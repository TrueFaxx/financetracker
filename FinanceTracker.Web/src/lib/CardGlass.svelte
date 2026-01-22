<script lang="ts">
  export let className = "";
</script>

<div
  class={`relative overflow-hidden rounded-2xl border border-white/10 bg-white/[0.05] backdrop-blur-xl
          shadow-[0_0_0_1px_rgba(255,255,255,0.02)] ${className}`}
>
  <div
    class="pointer-events-none absolute inset-0 rounded-2xl
           bg-[radial-gradient(1200px_circle_at_-20%_-10%,rgba(255,255,255,0.12),transparent_60%),
              radial-gradient(1000px_circle_at_120%_0%,rgba(99,102,241,0.16),transparent_55%),
              radial-gradient(900px_circle_at_50%_120%,rgba(236,72,153,0.10),transparent_60%)]"
  ></div>
  <div class="relative p-5">
    <slot />
  </div>
</div>
