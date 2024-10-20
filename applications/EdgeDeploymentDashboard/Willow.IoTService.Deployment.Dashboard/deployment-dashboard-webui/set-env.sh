#!/bin/sh

cp -f /usr/share/nginx/html/config-env.js /tmp

# Replace the placeholder variables in the config-env.js file with the values of the environment variables
for var in $(env | grep ^REACT_APP_ | awk -F'=' '{print $1}'); do
    placeholder=$(echo "$var" | sed -e "s/REACT_APP_/PH_/g")
    env_var_value=$(printenv "$var")
    sed -i -e "s|$placeholder|$env_var_value|g" /tmp/config-env.js
done

cat /tmp/config-env.js > /usr/share/nginx/html/config-env.js
