# Deploy Van Phuc Web on Kubernetes

This deployment uses one replica because the application is Blazor Server and
keeps SignalR circuit state in the web process. It is restarted automatically
when its health endpoint fails.

## One-time setup

Create the required secret without putting credentials in Git. Add every value
that must override `appsettings.json`, for example:

```bash
kubectl create namespace bv199 --dry-run=client -o yaml | kubectl apply -f -
kubectl -n bv199 create secret generic van-phuc-web-secrets \
  --from-literal=ConnectionStrings__UserDbConnectionString='REPLACE_ME' \
  --from-literal=FTPConfig__Host='REPLACE_ME' \
  --from-literal=FTPConfig__Username='REPLACE_ME' \
  --from-literal=FTPConfig__Password='REPLACE_ME' \
  --from-literal=FTPConfig__Folder='uploads' \
  --dry-run=client -o yaml | kubectl apply -f -
```

If the public image/download host differs from the application host, also add
`DrCoreApi__BaseUrlImage` to this secret with its public HTTPS URL.

## Deploy or update

```bash
kubectl apply -k kubernetes/
kubectl -n bv199 rollout status deployment/van-phuc-web
```

The image uses `bv199-latest` with `imagePullPolicy: Always`, so a restart after
CI/CD pulls the latest published image:

```bash
kubectl -n bv199 rollout restart deployment/van-phuc-web
kubectl -n bv199 rollout status deployment/van-phuc-web
```

Expose the `van-phuc-web` service through the cluster's existing Ingress or
load balancer. For Blazor Server, enable WebSocket support and sticky sessions
if you later increase replicas.
