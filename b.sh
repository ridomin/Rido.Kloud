#!/bin/bash

docker build -t k3d-registry.localhost:5500/ridohubproxy:latest -f Rido.HubProxy/Dockerfile .
docker push k3d-registry.localhost:5500/ridohubproxy:latest
kubectl apply -f ridohubproxy.yaml

docker build -t k3d-registry.localhost:5500/ridobrokerproxy:latest -f Rido.BrokerProxy/Dockerfile .
docker push k3d-registry.localhost:5500/ridobrokerproxy:latest
kubectl apply -f ridobrokerproxy.yaml
