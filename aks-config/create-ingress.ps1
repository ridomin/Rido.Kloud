# Create a namespace for ingress resources
kubectl create namespace ingress-basic

# Add the Helm repository
helm repo add ingress-nginx https://kubernetes.github.io/ingress-nginx
helm repo update

# Use Helm to deploy an NGINX ingress controller
helm install ingress-nginx ingress-nginx/ingress-nginx --namespace ingress-basic --set controller.replicaCount=2