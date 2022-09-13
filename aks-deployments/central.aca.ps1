az containerapp create `
    -n rido-devices `
    -g RidoEdge `
    --environment RidoEdge-Apps `
    --image ghcr.io/iotmodels/memmon:latest `
    --name centraldevice-aca `
    --env-vars "ConnectionStrings__cs=IdScope=0ne006CAFFC;SasMinutes=100;SharedAccessKey=invalid" "masterKey=vyB2VqViNdAILXTw5SRiKNiSrrfgPLOy9/sDu2d2ZaP/NSD3Y/0NNrnJ2O7S6wjdxLTCYTIpb3kNsHpipts7ew=="