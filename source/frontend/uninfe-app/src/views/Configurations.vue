<template>
  <validation-observer
    ref="observer"
    v-slot="{ invalid }"
  >
    <br>
    <p style="font-size:20px;"> Adicionar nova empresa: </p>

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

    <v-container>
      <v-row>  
        <v-col
          cols="12"
          sm="6"
          >
          <validation-provider
          v-slot="{ errors }"
          name="Documento"
          rules="required"
        >
          <v-select
            v-model="configuration.documentType"
            :items="itemsDoc"
            :error-messages="errors"
            label="Documento"
            data-vv-name="document"
            required
            outlined
          ></v-select>
        </validation-provider>
      </v-col>

      <v-col
        cols="12"
        sm="6"
        > 
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
      </v-col>
    </v-row>
    </v-container>

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

      <validation-provider
        v-slot="{ errors }"
        name="Serviço"
        rules="required"
      >
        <v-select
          v-model="configuration.service"
          :items="itemsService"
          :error-messages="errors"
          label="Serviço"
          data-vv-name="service"
          required
          outlined
        ></v-select>
      </validation-provider>

      <v-text-field 
        v-if="configuration.service == 2 || configuration.service == 9"
        v-model="configuration.csc"
        :rules="rules"
        counter
        outlined
        maxlength="50"
        hint="This field uses maxlength attribute"
        label="CSC"
      ></v-text-field>

      <v-text-field
        v-if="configuration.service == 2 || configuration.service == 9"
        v-model="configuration.idToken"
        :rules="rules"
        counter
        outlined
        maxlength="6"
        hint="This field uses maxlength attribute"
        label="ID Token"
      ></v-text-field>
      
        <validation-provider
        v-slot="{ errors }"
        name="Unidade Federativa"
        rules="required"
      >
        <v-select
          v-model="configuration.uf"
          :items="itemsUF"
          :error-messages="errors"
          label="Unidade Federativa"
          data-vv-name="uf"
          required
          outlined
        ></v-select>
      </validation-provider>

        <validation-provider
        v-slot="{ errors }"
        name="Ambiente"
        rules="required"
      >
        <v-select
          v-model="configuration.environment"
          :items="itemsEnvironment"
          :error-messages="errors"
          label="Ambiente"
          data-vv-name="environment"
          required
          outlined
        ></v-select>
      </validation-provider>

        <validation-provider
        v-slot="{ errors }"
        name="Tipo de Emissão"
        rules="required"
      >
        <v-select
          v-model="configuration.issuanceType"
          :items="itemsIssuanceType"
          :error-messages="errors"
          label="Tipo de Emissão"
          data-vv-name="issuanceType"
          required
          outlined
        ></v-select>
      </validation-provider>

      <v-card>
        <v-container
          fluid
        >
          <v-checkbox
            v-model="configuration.useCertificatedFromOs"
            label="Utilizar certificado digital instalado sistema operacional?"
            color="success"
          ></v-checkbox>
          <v-file-input
          v-if="configuration.useCertificatedFromOs"
          label="Certificado Digital"
          outlined
          dense
        ></v-file-input>
        </v-container>
      </v-card>
      <br>

      <p style="font-size:20px;"> Pastas </p>

      <v-text-field
        prepend-inner-icon="mdi-folder-open"
        label="Pasta onde serão gravados os arquivos XML's a serem enviados individualmente para os WebServices" 
        hint="Exemplo: C:\uninfe\Envio"
        persistent-hint
        outlined
      ></v-text-field><br>

      <v-text-field
        prepend-inner-icon="mdi-folder-open"
        label="Pasta onde serão gravados os arquivos XML's de retorno dos WebServices" 
        hint="Exemplo: C:\uninfe\Retorno"
        persistent-hint
        outlined
      ></v-text-field><br>

      <v-text-field
        prepend-inner-icon="mdi-folder-open"
        label="Pasta onde serão gravados os arquivos XML's enviados" 
        hint="Exemplo: C:\uninfe\Enviado"
        persistent-hint
        outlined
      ></v-text-field><br>

      <v-text-field
        prepend-inner-icon="mdi-folder-open"
        label="Pasta para arquivamento temporário dos XML's que apresentaram erro na tentativa do envio" 
        hint="Exemplo: C:\uninfe\Erro"
        persistent-hint
        outlined
      ></v-text-field><br>

      <v-text-field
        prepend-inner-icon="mdi-folder-open"
        label="Pasta para Backups dos XML's enviados" 
        hint="Exemplo: C:\uninfe\Backup"
        persistent-hint
        outlined
      ></v-text-field><br>

      <v-text-field
        prepend-inner-icon="mdi-folder-open"
        label="Pasta onde serão gravados somente validados" 
        hint="Exemplo: C:\uninfe\Validar"
        persistent-hint
        outlined
      ></v-text-field><br>

      <v-text-field
        prepend-inner-icon="mdi-folder-open"
        label="Pasta onde serão gravados os arquivos XML's de NFe baixados da Sefaz e retornos da consulta de eventos de terceiros" 
        hint="Exemplo: C:\uninfe\DownloadNFe"
        persistent-hint
        outlined
      ></v-text-field><br>    

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
      itemsDoc: [
          'CPF',
          'CNPJ',
          'CAEFP',
          'CEI'
        ],
      itemsService: [
          '1 NFe',
          '2 NFCe',
          '3 CTe/CTeOS',
          '4 MDFe',
          '5 GNRE',
          '9 Todos'
        ].map((itemService)=>{
            const serviceNumber = itemService.substring(0,1);
            const serviceName = itemService.substring(2, itemService.length);
            return {
              text: serviceName,
              value: serviceNumber
            }
          }),
      itemsUF: [
        '11 Rondônia-RO',
        '12 Acre-AC',
        '13 Amazonas-AM',
        '14 Roraima-RR',
        '15 Pará-PA',
        '16 Amapá-AP',
        '17 Tocantins-TO',
        '21 Maranhão-MA',
        '22 Piauí-PI',
        '23 Ceará-CE',
        '24 Rio Grande do Norte-RN',
        '25 Paraíba-PB',
        '26 Pernambuco-PE',
        '27 Alagoas-AL',
        '28 Sergipe-SE',
        '29 Bahia-BA',
        '31 Minas Gerais-MG',
        '32 Espírito Santo-ES',
        '33 Rio de Janeiro-RJ',
        '35 São Paulo-SP',
        '41 Paraná-PA',
        '42 Santa Catarina-SC',
        '43 Rio Grande do Sul-RS',
        '50 Mato Grosso do Sul-MS',
        '51 Mato Grosso-MT',
        '52 Goiás-GO',
        '53 Distrito Federal-DF'
        ].map((itemUF)=>{
            const ufNumber = itemUF.substring(0,2);
            const ufName = itemUF.substring(3, itemUF.length);
            return {
              text: ufName,
              value: ufNumber
            }
          }).sort(function(a, b) {
            return a.text.localeCompare(b.text);
          }),
      itemsEnvironment: [{
        text: "Produção",
        value: 1
        },
        {
        text: "Homologação",
        value: 2
        },
        ],
      itemsIssuanceType: [
        '1 Normal',
        '2 Contingência MDFe',
        '4 Contingência EPEC',
        '5 Contingência FS-DA',
        '6 Contingência SVCAN',
        '7 Contingência SVCRS',
        '8 Contingência SVCSP'
        ].map((itemIssuanceType)=>{
            const issuanceTypeNumber = itemIssuanceType.substring(0,1);
            const issuanceTypeName = itemIssuanceType.substring(2, itemIssuanceType.length);
            return {
              text: issuanceTypeName,
              value: issuanceTypeNumber
            }
          }),
      success: false,  
      configuration: {
        id: undefined,
        name: '',
        documentNumber: '',
        email: '',
        documentType: null,
        service: null,
        csc: '',
        idToken: '',
        uf: null,
        environment: null,
        issuanceType: null,
        useCertificatedFromOs: false,
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
        console.log("Form value", this.configuration)
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
        this.service = null
        this.csc = ''
        this.idToken = ''
        this.uf = null
        this.environment = null
        this.issuanceType = null
        this.useCertificatedFromOs = null;
        this.success = false
        this.$refs.observer.reset()
      },
    },
  }
</script>