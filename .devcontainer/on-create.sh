#!/bin/bash

# this runs as part of pre-build

echo "on-create start"
echo "$(date +'%Y-%m-%d %H:%M:%S')    on-create start" >> "$HOME/status"

export REPO_BASE=$PWD
export PATH="$PATH:$REPO_BASE/bin"

mkdir -p "$HOME/.ssh"
mkdir -p "$HOME/.oh-my-zsh/completions"

{
    # add cli to path
    echo "export PATH=\$PATH:$REPO_BASE/bin"

    echo "export REPO_BASE=$REPO_BASE"
    echo "compinit"
} >> "$HOME/.zshrc"

# restore the repos

# make sure everything is up to date
sudo apt-get update
sudo apt-get upgrade -y
sudo apt-get autoremove -y
sudo apt-get clean -y

# create local registry
docker network create k3d
k3d registry create registry.localhost --port 5500
docker network connect k3d k3d-registry.localhost

# update the base docker images
docker pull mcr.microsoft.com/dotnet/aspnet:6.0-alpine
docker pull mcr.microsoft.com/dotnet/sdk:6.0

echo "creating k3d cluster"
k3d cluster create --registry-use k3d-registry.localhost:5500 --config "$REPO_BASE/.devcontainer/k3d.yaml"
kubectl wait node --for condition=ready --all --timeout=30s
sleep 10
kubectl wait pod -l k8s-app=kube-dns -n kube-system --for condition=ready --timeout 30s

echo "on-create complete"
echo "$(date +'%Y-%m-%d %H:%M:%S')    on-create complete" >> "$HOME/status"
