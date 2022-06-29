# Label the cert-manager namespace to disable resource validation
kubectl label namespace ingress-basic cert-manager.io/disable-validation=true

# Add the Jetstack Helm repository
helm repo add jetstack https://charts.jetstack.io
# Update your local Helm chart repository cache
helm repo update

# Install CRDs with kubectl
kubectl apply -f https://github.com/cert-manager/cert-manager/releases/download/v1.7.1/cert-manager.crds.yaml
# Install the cert-manager Helm chart
helm install cert-manager jetstack/cert-manager --namespace ingress-basic --version v1.7.1