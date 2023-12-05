Como executar o RabbitMQ:

```
docker run -d --hostname my-rabbit --name some-rabbit -p 5672:5672 -p 15672:15672 rabbitmq:3-management
```

Altera as variáveis de ambiente dos projetos para se adequar as suas configurações de Mongo e PostgreSQL.
