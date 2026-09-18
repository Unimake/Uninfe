# Auditoria de integração UAB-006

O diff da etapa está restrito a testes NF-e ABI, exemplo sintético, documentação/índices e governança. Nenhum código de produção foi alterado nesta etapa.

Os contratos públicos documentados foram conferidos contra o código: autorização `-nfeabi.xml`, `-ret-nfeabi.xml`, `-ret-nfeabi.err` e `-procNFeABI.xml`; status `-ped-sta.xml`, `-sta.xml` e `-sta.err`. Os destinos autorizados respeitam a subpasta por data configurada. A recuperação após `.err` não orienta retransmissão quando há retorno/evidência pendente ou estado remoto incerto.

O exemplo usa somente dados sintéticos, namespace oficial, versão 1.00, modelo 77 e homologação. O teste o copia como fixture, prepara e valida a assinatura com certificado temporário, sem executar transporte. A documentação mantém produção fail-closed e declara consulta de protocolo e eventos indisponíveis. Não foram criados endpoint, serviço futuro, dependência ou contrato ERP novo.
