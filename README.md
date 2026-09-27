## Better Boarding

Better Boarding reduces public transport boarding delays without replacing the game's transport AI.

The game can keep a transit vehicle stuck boarding past departure time while it waits for a late passenger — even if that cim is still far away. Vanilla only releases a stuck transit vehicle after ~10 in-game minutes.

Better Boarding helps much sooner by improving late-passenger behavior while leaving the game's routes, schedules, and un-bunching systems in control.

### What It Does

- Faster boarding/loading sliders for bus, rail, ship + ferry, and airplane.
- `Skip Late Passengers`: late solo cims can miss a vehicle after a short grace instead of holding everyone.
- Groups/families get extra help so one lagging child, pet, or group member does not cause a long delay.
- `Cims Run Sooner`: assigned late passengers start running earlier for bus, tram, train, and subway.
- Compact Options status plus `Stats to Log` for waits, worst stops, skipped passengers, and troubleshooting.

### Late Passengers

Better Boarding mainly targets citizen behavior because the schedule does not help much if a cim several blocks away can keep a vehicle from leaving.

- Late solo passengers can miss the vehicle after departure instead of holding everyone.
- Skipped cims are not deleted; vanilla can naturally reassign or reroute them.
- If a family/group leader is still outside, the whole group can be released from that vehicle using vanilla behavior.
- If the leader is already aboard, Better Boarding helps vanilla finish boarding a lagging child or pet.

### Cims Run Sooner

Vanilla normally starts assigned late cims running at departure time, which can already be too late.

Better Boarding lets assigned passengers start running 512 frames earlier for:

- Bus, Tram, Train, or Subway

It does not change schedules or departure times, force boarding, or teleport citizens.

### Safe Design

Better Boarding does not use Harmony and does not replace vanilla transport systems.

- Vanilla still controls routes, departure scheduling, and un-bunching.
- Vanilla may intentionally hold a vehicle a little longer to improve spacing; that is normal.
- Better Boarding mainly steps in when late passengers would otherwise keep boarding open too long.
- Save-game safe and safe to remove anytime.

### Tips

- If a stop has huge waits, use `Stats to Log` and check the worst stops for traffic, bad placement, too few vehicles, or a bugged stop.
- `Stats to Log` creates a one-time snapshot only when clicked; it is not a continuous per-frame system.
- Keep verbose logging OFF during normal gameplay. It is for testing and can create large log files.

### Compatibility

Use only one boarding-behavior mod at a time. Do not use Better Boarding and All Aboard together.

Some players report no issues using Smart Transportation together with Better Boarding.

### Credits

- River-Mochi: mod author
- bcallender's All Aboard and Wayze's InstantBoarding: inspiration
- yenyang: testing and code feedback
- 🎀 foxxy ✿, MayorCheeks, Gagaxm, Neco1996, Empiiey: testing
- elGendo87: Spanish editor, thumbnail straightener, testing
