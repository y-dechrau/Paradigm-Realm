namespace ParadigmRealm {
// Early-game opinion on the -100 to +100 scale (owner brief, 27 Nov 2026). At turn 47 two neighbours showed +33 and +80 with the
// same smiling mask; the probe of the owner's turn-50 save found the +80 was an envoy (+30), three pacts (+20), the difficulty
// (+10, labelled "Wary of you" though it was a bonus), trade (+7), trust counted again (+5), the same government (+5), a long
// friendship (+3) and a ruler's mood (+3): almost nothing they had been through together. Now routine contact is capped and
// meaningful history (kept word, help in need, fulfilled agreements, long friendship) is what lifts a leader towards +80.
public static class OpinionTuning {
 public const int PerPeacePact=3,Coexistence=5,Trade=4,Dealings=8,Gifts=6,Envoy=6,Alliance=6,SameGovernment=2;
 public const int Friendship=10,FriendshipTurns=10;      // +1 per 10 turns of agreements (by game speed), at most +10
 public const int ProvenTrust=60;                        // how far kept word and help can raise trust above what paper allows
 public const int FulfilledTrust=10,FulfilledOpinion=8;  // a fulfilled supply agreement: trust and a kept-word mark
}
}
