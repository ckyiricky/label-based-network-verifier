[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg?style=popout)](https://opensource.org/licenses/MIT)
[![codecov](https://codecov.io/gh/ckyiricky/label-based-network-verifier/branch/master/graph/badge.svg?token=1W5VPUYXWJ)](https://codecov.io/gh/ckyiricky/label-based-network-verifier)

# Introduction

This project implements a label based network verifier based on [ZenLib](https://github.com/microsoft/Zen).

It verifies network connections and checks violations in k8s clusters based on Pod/Network Policy/Namespace.

# Usage

All public methods of Kano has been implemented in this project (except the policy conflict which is problematic):

- *CreateReachMatrix* takes all pods, policies and namespaces as input and
  returns 5 matrices including ingress and egress reachability matrices
  described above, three BCP matrices that map bi-directional relations between
  pods and policies (BCP matrices are used for policy shadow check).
- *AllReachableCheck* takes ingress reachability matrix and a flag (indicates
  if it’s all reachable or all isolated check) as input and returns a list of
  indices of satisfied pods.
- *UserCrossCheck* takes ingress reachability matrix, user-pod matrix (this can
  be produced from GetUserHash function), all pods and key of user label as
  input and returns a list of indices of pods that can only be reached from the
  same users’ pods.
- *SystemIsolationCheck* takes egress reachability matrix and a pod index as
  input and returns a list of indices of pods that can’t be reached from the
  input pod.
- *PolicyShadowCheck* takes three BCP matrices as input and returns a list of
  tuples of indices where the second index is the policy shadowed by the first
  one.

All algorithms and violation checkers are covered with their corresponding unit tests where detailed usage and data generation
can be found.
