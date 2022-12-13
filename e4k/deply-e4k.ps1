helm install e4k oci://e4kpreview.azurecr.io/helm/az-e4k `
    --version 0.1.0-amd64 `
    -f my-values.yaml `
    --set-file=e4kdmqtt.authentication.x509.clientTrustedRoots=RidoFY23CA.pem `
    --set e4kdmqtt.broker.backend.chainCount=1