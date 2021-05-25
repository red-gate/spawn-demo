# See here for image contents: https://github.com/microsoft/vscode-dev-containers/tree/v0.140.1/containers/dotnetcore/.devcontainer/base.Dockerfile
ARG VARIANT="3.1"
FROM mcr.microsoft.com/vscode/devcontainers/dotnetcore:0-${VARIANT}

ARG INSTALL_NODE="true"
ARG NODE_VERSION="lts/*"
RUN if [ "${INSTALL_NODE}" = "true" ]; then su vscode -c "source /usr/local/share/nvm/nvm.sh && nvm install ${NODE_VERSION} 2>&1"; fi

# Install spawnctl
RUN curl https://run.spawn.cc/install | sh && ln -s $HOME/.spawnctl/bin/spawnctl /usr/local/bin

# Install Flyway
RUN wget -qO- https://repo1.maven.org/maven2/org/flywaydb/flyway-commandline/7.0.2/flyway-commandline-7.0.2-linux-x64.tar.gz | \
 tar xvz && sudo ln -s `pwd`/flyway-7.0.2/flyway /usr/local/bin

# And bash completion
RUN apt update
RUN apt install bash-completion
RUN echo "source /etc/profile.d/bash_completion.sh" >> ~/.bashrc
RUN echo "source <(spawnctl completion bash)" >> ~/.bashrc
