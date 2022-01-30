# ts.hermanj.no bot

This repo contains a discord bot written with [Discord.Net](https://github.com/discord-net/Discord.Net).
The goal of the bot is to make our transition from TeamSpeak to Discord as smooth as possible.

## Features

### Voice Text Channels

The bot will create text channels for each voice channel, which only the participants of the voice channel will have access to.
This is similar to the way TeamSpeak handles text channels.

### Lock Voice Channels

When typing `!lock` in a Voice Text Channel, the user limit of the Voice Channel will be set to the current amount of users (also updated when a user leaves).
When all users leave the voice channel, or someone types `!unlock`, the user limit will be set to 99 again.

### Custom Colors

React to a message from the bot to set a custom color in the user list.

### Daily Wordle Thread

Creates a thread every day for everyone to post their Wordle result.