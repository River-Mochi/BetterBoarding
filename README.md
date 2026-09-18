## Better Boarding

Better Boarding helps public transport board faster without replacing the game's transport AI.

### What It Does

- Faster boarding/loading sliders for bus, rail, ship + ferry, and airplane.
- `Skip Late Passengers`: solo cims who are still late after departure time can miss that vehicle instead of holding everyone.
- `Cims Run Sooner: Bus + Tram + Train`: assigned late passengers start running before departure, which is especially helpful on long train platforms.
- Compact Options status for current waits, worst stops, `late today`, and run-sooner counts.
- `Stats to Log` writes a detailed troubleshooting report with stop names, entity IDs, line hints, and worst stops.

### Safe Design

Better Boarding does not use Harmony and does not replace vanilla transport systems.

- Sliders tune vanilla passenger stop data.
- Skipped cims are not deleted; vanilla can reassign or reroute them.
- Groups/families are left to vanilla to avoid splitting up the travelers (they can still slow down boarding, but are few compared to many single travelers).
- Save-game safe and safe to remove anytime.

### Tips

- If a stop still has huge waits, use `Stats to Log` and inspect the worst stops for traffic, bad stop placement, or not enough vehicles.
- Keep verbose logging OFF during normal gameplay; it is for testing. Leaving it on can cause performance issues and huge log file spam.

### Compatibility

Use only one boarding-behavior mod at a time, if you use this mod, don't use All Aboard at the same time.

### Credits

- River-Mochi: mod author
- bcallender's All Aboard and Wayze's InstantBoarding: inspiration
- yenyang: testing and code feedback
- Neco1996: testing
- MayorCheeks: testing
- gagaxm: testing
- Empiiey: testing
- elGaucho87: thumbnail straightener
