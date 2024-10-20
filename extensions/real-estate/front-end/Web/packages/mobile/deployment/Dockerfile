# Dockerfile used for hosting of Ephemeral development environments
# https://github.com/WillowInc/rfcs/discussions/59
FROM nginx:1.27-alpine

RUN chown -R nginx:nginx /var/cache/nginx && \
    chown -R nginx:nginx /var/log/nginx && \
    chown -R nginx:nginx /etc/nginx/conf.d && \
    chown -R nginx:nginx /usr/share/nginx/html
RUN touch /var/run/nginx.pid && \
    chown -R nginx:nginx /var/run/nginx.pid

ENV BUILD_DEPS="gettext"  \
    RUNTIME_DEPS="libintl"

RUN set -x && \
    apk add --update $RUNTIME_DEPS && \
    apk add --virtual build_deps $BUILD_DEPS &&  \
    cp /usr/bin/envsubst /usr/local/bin/envsubst && \
    apk del build_deps

COPY deployment/nginx.conf /etc/nginx/nginx.conf
COPY --chown=nginx:nginx ./dist /usr/share/nginx/html

USER nginx:nginx
EXPOSE 8082
CMD ["/bin/sh", "-c", "envsubst < /usr/share/nginx/html/public/config-env.json > /usr/share/nginx/html/public/config.json && nginx -g \"daemon off;\""]
