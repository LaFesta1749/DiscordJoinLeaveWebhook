An Exiled plugin for SCP:Secret Labotary that sends a discord webhook containing the message when a player joins and leaves the server. Including a timespan of 5 minutes if the player decides to quit and rejoin to avoid becoming a 049-2 (zombie)

Example config:

discord_join_leave_webhook:
  is_enabled: true
  webhook_url: 'https://discord.com/api/webhooks/1278282849899905126/ECg4kVvIzwhC8C60s4ig7TZfMK2_qysRewBFhI5RLrLkQWLnA-OeyMWff0LU43TD6_zg'
  debug: false
  time_since_leaving_counter: 5
  show_player_i_p: false
  join_message: 'joined the server.'
  leave_message: 'left the server.'
  last_role: 'Role'
  timer: 'Last left the server {minutes} minutes ago.'
