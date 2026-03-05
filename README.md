# BroadcastStack

EXILED plugin that stacks broadcasts on top of each other instead of replacing them.

Normally when two broadcasts fire at the same time, the second one wipes the first. This plugin intercepts all broadcast paths and combines them into a single stacked display, so nothing gets lost.

## Requirements

- EXILED 9.13.1+

## Installation

Drop `BroadcastStack.dll` into your `EXILED/Plugins/` folder and restart.

## Config

```yaml
broadcast_stack:
  is_enabled: true
  debug: false
  flags: Normal
  clear_on_should_clear_previous: true
  ignore_empty: true
  stack_staff_chat: true
  staff_chat_duration: 5
  staff_chat_prefix: '<size=20>[Staff]</size>'
  show_staff_chat_sender_name: true
```

| Key | Default | Description |
|---|---|---|
| `flags` | `Normal` | Broadcast flags used for stacked messages |
| `clear_on_should_clear_previous` | `true` | Honor clear requests from other plugins |
| `ignore_empty` | `true` | Drop blank messages |
| `stack_staff_chat` | `true` | Mirror staff chat as a broadcast overlay (RA only) |
| `staff_chat_duration` | `5` | How long staff chat broadcasts stay on screen |
| `staff_chat_prefix` | `[Staff]` | Prefix shown before staff chat messages |
| `show_staff_chat_sender_name` | `true` | Show the sender's display name next to the prefix |

## Notes
credit to [Indet2008](https://github.com/Indet2008/BroadcastStackingPlugin) for the original idea
