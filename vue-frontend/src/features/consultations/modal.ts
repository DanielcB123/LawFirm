import { computed, ref } from 'vue'

const isOpenRef = ref(false)
const openerRef = ref<HTMLElement | null>(null)

function openModal(opener?: EventTarget | null) {
  if (opener instanceof HTMLElement) {
    openerRef.value = opener
  } else {
    openerRef.value = null
  }

  isOpenRef.value = true
}

function closeModal() {
  isOpenRef.value = false
  if (openerRef.value) {
    openerRef.value.focus()
  }
}

export function useConsultationModal() {
  return {
    isOpen: computed(() => isOpenRef.value),
    openModal,
    closeModal,
  }
}
