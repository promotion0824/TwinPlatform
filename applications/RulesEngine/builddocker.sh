echo Do these on a Mac to ensure nginx is running to redirect localhost to the VM / K8s
echo sudo chown root:wheel homebrew.mxcl.nginx.plist
echo sudo chmod 600 sudo chown homebrew.mxcl.nginx.plist
echo sudo launchctl load /usr/local/cellar/nginx/1.21.6_1/homebrew.mxcl.nginx.plist 
echo
echo Switching to MICROK8s - kubectl roll out needs to happen there not on any PROD or UAT environment!!
echo
kubectl config use-context microk8s
echo
echo
docker build . -f RulesEngine.Processor/Dockerfile.local -t localregistry:32000/rules/processor
docker push localregistry:32000/rules/processor
echo roll out prod-processor
kubectl rollout restart deployment/microsoft-prod-processor --namespace=microsoft-prod
echo
echo
docker build . -f RulesEngine.Web/Dockerfile.ReactApp -t localregistry:32000/rules/react
docker push localregistry:32000/rules/react
echo roll out prod-react
kubectl rollout restart deployment/microsoft-prod-react --namespace=microsoft-prod
kubectl rollout restart deployment/investa-prod-react --namespace=investa-prod
echo
echo
docker build . -f RulesEngine.Web/Dockerfile.local -t localregistry:32000/rules/web
docker push localregistry:32000/rules/web
echo roll out prod-web
kubectl rollout restart deployment/microsoft-prod-web --namespace=microsoft-prod
kubectl rollout restart deployment/investa-prod-web --namespace=investa-prod
echo

