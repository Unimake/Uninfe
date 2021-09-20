<template>
  <validation-observer
    ref="observer"
    v-slot="{ invalid }"
  >
    <br>
    <p style="font-size:20px;"> Adicionar nova empresa: </p>
    <br>

    <form @submit.prevent="submit">

      <validation-provider
        v-slot="{ errors }"
        name="Nome"
        rules="required"
      >
        <v-text-field
          v-model="configuration.name"
          :error-messages="errors"
          label="Nome"
          required
          outlined
        ></v-text-field>
      </validation-provider>

        <validation-provider
        v-slot="{ errors }"
        name="Documento"
        rules="required"
      >
        <v-select
          v-model="configuration.documentType"
          :items="items"
          :error-messages="errors"
          label="Documento"
          data-vv-name="document"
          required
          outlined
        ></v-select>
      </validation-provider>

      <validation-provider
        v-slot="{ errors }"
        name="Número de documento"
        rules="required"
      >
        <v-text-field
          v-model="configuration.documentNumber"
          :error-messages="errors"
          label="Número"
          required
          outlined
        ></v-text-field>
      </validation-provider>

      <validation-provider
        v-slot="{ errors }"
        name="e-mail"
        rules="required|email"
      >
        <v-text-field
          v-model="configuration.email"
          :error-messages="errors"
          label="E-mail"
          required
          outlined
        ></v-text-field>
      </validation-provider>

      <v-btn
        class="mr-4"
        type="submit"
        color="success"
        :disabled="invalid"
      >
        Ok
      </v-btn>
      <v-alert
        type="success"
        v-if="success"
      >Empresa atualizada</v-alert>

      <v-btn @click="clear">
        Limpar
      </v-btn>
    </form>
  </validation-observer>
</template>

<script>
  import { required, digits, email, max, regex } from 'vee-validate/dist/rules'
  import { extend, ValidationObserver, ValidationProvider, setInteractionMode } from 'vee-validate'
  import axios from 'axios'

  setInteractionMode('eager')

  extend('digits', {
    ...digits,
    message: '{_field_} needs to be {length} digits. ({_value_})',
  })

  extend('required', {
    ...required,
    message: 'Por favor, adicione um {_field_}',
  })

  extend('max', {
    ...max,
    message: '{_field_} may not be greater than {length} characters',
  })

  extend('regex', {
    ...regex,
    message: '{_field_} {_value_} does not match {regex}',
  })

  extend('email', {
    ...email,
    message: 'Adicione um e-mail válido',
  })

  export default {
    components: {
      ValidationProvider,
      ValidationObserver,
    },
    data: () => ({
      items: [
          'CPF',
          'CNPJ',
          'CAEFP',
          'CEI'
        ],
      success: false,  
      configuration: {
        id: undefined,
        name: '',
        documentNumber: '',
        email: '',
        documentType: null,
    }}),

    mounted () {
      axios
        .get('http://localhost:5000/api/Configuration')
        .then(response => {
          if (response.data.length > 0) {
            this.configuration = response.data[0]
          }        
        })
    },

    methods: {
      submit () {
        this.$refs.observer.validate()
        
        if (this.configuration.id == undefined) {
          axios
            .post('http://localhost:5000/api/Configuration', this.configuration)
            .then(response => {
              this.configuration = response.data
              this.success = true
          })  
        } else {
          axios
            .put(`http://localhost:5000/api/Configuration/${this.configuration.id}`, this.configuration)
            .then(response => {
              this.configuration = response.data
              this.success = true
            })  
          }
      },
      clear () {
        this.configuration.name = ''
        this.configuration.documentNumber = ''
        this.configuration.email = ''
        this.configuration.documentType = null
        this.success = false
        this.$refs.observer.reset()
      },
    },
  }
</script>