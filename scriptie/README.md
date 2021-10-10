# Scriptie
In deze directory staat alles om de scriptie op te bouwen.

## Omgeving opzetten

Zorg dat je in de map scriptie staat

### Install drawio
```
snap install drawio
```

### Maak python omgeving met dependencies
```
python3 -m venv .pyenv-scriptie
pip3 install -r requirements.txt
```

### Login op overleaf
```
ols login
```
Dit maakt een .olauth file aan. Deze staat ook in de .gitignore.

### Losse commando's
Synchroniseren met overleaf
```
ols --path overleaf --name Afstudeerverslag
```
Images builden
```
drawio -x scriptie/images/drawio -f png -o scriptie/overleaf/images/
```
