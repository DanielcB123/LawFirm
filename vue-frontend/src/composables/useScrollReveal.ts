import { nextTick, onBeforeUnmount, onMounted, watch, type WatchSource } from 'vue'

export function useScrollReveal(trigger: WatchSource<string>) {
  let observer: IntersectionObserver | null = null

  const setupObserver = async () => {
    await nextTick()

    observer?.disconnect()

    const elements = Array.from(document.querySelectorAll<HTMLElement>('[data-reveal]'))
    if (elements.length === 0) {
      return
    }

    const reducedMotion = window.matchMedia('(prefers-reduced-motion: reduce)').matches
    if (reducedMotion) {
      for (const element of elements) {
        element.classList.add('is-revealed')
      }
      return
    }

    observer = new IntersectionObserver(
      (entries) => {
        for (const entry of entries) {
          if (entry.isIntersecting) {
            entry.target.classList.add('is-revealed')
          }
        }
      },
      { threshold: 0.14, rootMargin: '0px 0px -8% 0px' },
    )

    for (const element of elements) {
      element.classList.remove('is-revealed')
      observer.observe(element)
    }
  }

  onMounted(() => {
    void setupObserver()
  })

  watch(trigger, () => {
    void setupObserver()
  })

  onBeforeUnmount(() => {
    observer?.disconnect()
    observer = null
  })
}
