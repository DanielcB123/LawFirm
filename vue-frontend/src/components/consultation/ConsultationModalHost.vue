<script setup lang="ts">
import ConsultationModal from './ConsultationModal.vue'
import { useConsultationModal } from '../../features/consultations/modal'

const { isOpen, closeModal } = useConsultationModal()
</script>

<template>
  <ConsultationModal v-if="isOpen" @close="closeModal" />
</template>
