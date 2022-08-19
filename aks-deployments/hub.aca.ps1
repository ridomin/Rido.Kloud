az containerapp create `
    -n rido-devices `
    -g RidoEdge `
    --environment RidoEdge-Apps `
    --image ghcr.io/ridomin/memmon:latest `
    --name hubdevice-aca `
    --env-vars "ConnectionStrings__cs=IdScope=0ne006CCDE4;SasMinutes=100;SharedAccessKey=invalid" "masterKey=/nH1Y+J9WVTi1tyxgS0Yv9Y6yxSITLBiG1v9C/KvtvEPkfV7sCFaJVSCRiGVdslEPwGrf3K9VFo0S1xEicMPLw=="