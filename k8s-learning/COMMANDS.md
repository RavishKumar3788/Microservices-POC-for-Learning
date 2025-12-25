# Kubernetes Commands Quick Reference

## Basic Commands

### View Resources
```powershell
# View all resources in namespace
kubectl get all -n microservices-poc

# View specific resource types
kubectl get pods -n microservices-poc
kubectl get deployments -n microservices-poc
kubectl get services -n microservices-poc
kubectl get pvc -n microservices-poc

# Watch resources (auto-refresh)
kubectl get pods -n microservices-poc -w
```

### Check Pod Details
```powershell
# Detailed information about a pod
kubectl describe pod <pod-name> -n microservices-poc

# View pod logs
kubectl logs <pod-name> -n microservices-poc

# Follow logs (live)
kubectl logs -f <pod-name> -n microservices-poc

# Logs from all pods of a deployment
kubectl logs -f deployment/products-app -n microservices-poc

# View previous logs (if pod crashed)
kubectl logs <pod-name> --previous -n microservices-poc
```

### Execute Commands in Pods
```powershell
# Open shell in a pod
kubectl exec -it <pod-name> -n microservices-poc -- /bin/sh

# Run a single command
kubectl exec <pod-name> -n microservices-poc -- ls /app

# Test MongoDB connection
kubectl exec -it deployment/mongodb -n microservices-poc -- mongosh
```

### Port Forwarding
```powershell
# Forward local port to service
kubectl port-forward -n microservices-poc service/nginx-proxy 8080:80

# Forward to specific pod
kubectl port-forward -n microservices-poc <pod-name> 8080:80

# Access multiple services
kubectl port-forward -n microservices-poc service/mongodb 27017:27017
kubectl port-forward -n microservices-poc service/redis 6379:6379
```

## Deployment Management

### Scaling
```powershell
# Scale deployment
kubectl scale deployment products-app --replicas=3 -n microservices-poc

# Scale multiple deployments
kubectl scale deployment products-app user-app orders-app --replicas=3 -n microservices-poc

# View current replica count
kubectl get deployment products-app -n microservices-poc
```

### Update Image
```powershell
# Update to new image version
kubectl set image deployment/products-app products-app=ravishchauhan/products-app:v2 -n microservices-poc

# Check rollout status
kubectl rollout status deployment/products-app -n microservices-poc

# View rollout history
kubectl rollout history deployment/products-app -n microservices-poc

# Rollback to previous version
kubectl rollout undo deployment/products-app -n microservices-poc

# Rollback to specific revision
kubectl rollout undo deployment/products-app --to-revision=2 -n microservices-poc
```

### Restart Deployment
```powershell
# Restart all pods in deployment (rolling restart)
kubectl rollout restart deployment/products-app -n microservices-poc
```

### Update Configuration
```powershell
# Edit deployment directly
kubectl edit deployment products-app -n microservices-poc

# Edit service
kubectl edit service products-app -n microservices-poc

# Update from file
kubectl apply -f step8-products-app.yaml
```

## Troubleshooting

### Pod Not Starting
```powershell
# 1. Check pod status
kubectl get pods -n microservices-poc

# 2. Describe pod for events
kubectl describe pod <pod-name> -n microservices-poc

# 3. Check logs
kubectl logs <pod-name> -n microservices-poc

# 4. Check previous logs if crashed
kubectl logs <pod-name> --previous -n microservices-poc
```

### Common Pod States
- **Pending**: Waiting to be scheduled (check: insufficient resources?)
- **ContainerCreating**: Pulling image (wait or check image name)
- **Running**: Everything is good!
- **CrashLoopBackOff**: Container keeps crashing (check logs)
- **ImagePullBackOff**: Can't pull image (check image name and registry)
- **Error**: Container exited with error (check logs)

### Check Resource Usage
```powershell
# Pod resource usage (requires metrics-server)
kubectl top pods -n microservices-poc

# Node resource usage
kubectl top nodes

# Describe node to see allocated resources
kubectl describe node
```

### Network Testing
```powershell
# Create debug pod
kubectl run debug --image=busybox -it --rm -n microservices-poc -- /bin/sh

# Test DNS resolution
kubectl run debug --image=busybox -it --rm -n microservices-poc -- nslookup products-app

# Test connectivity
kubectl run debug --image=curlimages/curl -it --rm -n microservices-poc -- curl http://products-app:8080/health
```

### View Events
```powershell
# All events in namespace
kubectl get events -n microservices-poc --sort-by='.lastTimestamp'

# Events for specific pod
kubectl get events --field-selector involvedObject.name=<pod-name> -n microservices-poc
```

## ConfigMaps and Secrets

### View ConfigMaps
```powershell
# List ConfigMaps
kubectl get configmaps -n microservices-poc

# View ConfigMap content
kubectl describe configmap nginx-config -n microservices-poc

# Get ConfigMap as YAML
kubectl get configmap nginx-config -n microservices-poc -o yaml
```

### Update ConfigMap
```powershell
# Edit ConfigMap
kubectl edit configmap nginx-config -n microservices-poc

# Update from file
kubectl apply -f step2-configmaps.yaml

# Restart pods to pick up new config
kubectl rollout restart deployment/nginx-proxy -n microservices-poc
```

## Storage

### View Storage
```powershell
# PersistentVolumeClaims
kubectl get pvc -n microservices-poc

# PersistentVolumes
kubectl get pv

# Describe PVC
kubectl describe pvc mongodb-pvc -n microservices-poc
```

## Cleanup

### Delete Specific Resources
```powershell
# Delete deployment
kubectl delete deployment products-app -n microservices-poc

# Delete service
kubectl delete service products-app -n microservices-poc

# Delete from file
kubectl delete -f step8-products-app.yaml
```

### Delete Everything
```powershell
# Delete entire namespace (removes everything)
kubectl delete namespace microservices-poc

# Or run cleanup script
.\cleanup.ps1
```

## Useful Tips

### Set Default Namespace
```powershell
# Set default namespace (avoid typing -n every time)
kubectl config set-context --current --namespace=microservices-poc

# Now you can use:
kubectl get pods
# Instead of:
kubectl get pods -n microservices-poc
```

### Output Formats
```powershell
# JSON output
kubectl get pods -n microservices-poc -o json

# YAML output
kubectl get pods -n microservices-poc -o yaml

# Wide output (more columns)
kubectl get pods -n microservices-poc -o wide

# Custom columns
kubectl get pods -n microservices-poc -o custom-columns=NAME:.metadata.name,STATUS:.status.phase
```

### Labels and Selectors
```powershell
# Get pods with specific label
kubectl get pods -l app=products-app -n microservices-poc
kubectl get pods -l tier=backend -n microservices-poc

# Get all backend services
kubectl get all -l tier=backend -n microservices-poc

# Add label to pod
kubectl label pod <pod-name> environment=production -n microservices-poc
```

### Copy Files
```powershell
# Copy file from pod to local
kubectl cp microservices-poc/<pod-name>:/path/to/file ./local-file

# Copy file from local to pod
kubectl cp ./local-file microservices-poc/<pod-name>:/path/to/file
```

## Learning Resources

- Official Docs: https://kubernetes.io/docs/
- kubectl Cheat Sheet: https://kubernetes.io/docs/reference/kubectl/cheatsheet/
- Interactive Tutorial: https://kubernetes.io/docs/tutorials/kubernetes-basics/
