# Paradigm Realm project workflow

Canonical repository: https://github.com/y-dechrau/Paradigm-Realm
Unity project: Unity/

Matt requests a GitHub backup whenever the assistant updates Paradigm Realm. After completing and verifying an authorised change, commit and push it to this repository as part of that task. Inspect the current branch and preserve unrelated work; do not force-push or rewrite history. If pushing fails, clearly tell Matt that the backup did not complete. Verify the remote commit before reporting success.

This workflow does not monitor or automatically upload changes made on Matt's PC. Those require GitHub Desktop or another local Git client.

Keep implemented features separate from planned features in Unity/Design/IMPLEMENTATION-STATUS.md. Do not describe pure C# checks or API-double compilation as Unity playtesting. The foundation alpha still needs real Unity UI and save/load validation.
