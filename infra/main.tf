data "hcloud_ssh_key" "primary-ssh-key" {
  fingerprint = "6d:8f:78:4e:5f:ba:d9:d0:d5:24:97:1c:a9:c2:95:d9"
}

resource "hcloud_network" "network" {
  name     = "network"
  ip_range = "10.0.0.0/16"
}

resource "hcloud_network_subnet" "nodes-subnet" {
  type         = "cloud"
  network_id   = hcloud_network.network.id
  network_zone = "eu-central"
  ip_range     = "10.0.1.0/24"
}

resource "hcloud_firewall" "firewall-ingress" {
  name   = "firewall-ingress"

  rule {
    destination_ips = []
    direction       = "in"
    port            = "22"
    protocol        = "tcp"
    source_ips = [
      "0.0.0.0/0",
      "::/0",
    ]
  }

  rule {
    destination_ips = []
    direction       = "in"
    port            = "443"
    protocol        = "tcp"
    source_ips = [
      "0.0.0.0/0",
      "::/0",
    ]
  }
}

resource "hcloud_firewall" "firewall-egress" {
  name   = "firewall-egress"

  rule {
    direction       = "out"
    port            = "53"
    protocol        = "tcp"
    destination_ips = [
      "0.0.0.0/0",
      "::/0",
    ]
  }

  rule {
    direction       = "out"
    port            = "53"
    protocol        = "udp"
    destination_ips = [
      "0.0.0.0/0",
      "::/0",
    ]
  }
}

# Place servers in different locations in the eu-cental region to improve fault tolerance

resource "hcloud_server" "node1" {
  name        = "node1"
  server_type = "cx21"
  image       = "ubuntu-22.04"
  location    = "hel1"
  ssh_keys    = [data.hcloud_ssh_key.primary-ssh-key.name]

  network {
    network_id = hcloud_network.network.id
    ip         = "10.0.1.1"
  }

  labels = {
    purpose = "nomad_node"
  }

  depends_on = [
    hcloud_network_subnet.nodes-subnet
  ]
}

resource "hcloud_server" "node2" {
  name        = "node2"
  server_type = "cx21"
  image       = "ubuntu-22.04"
  location    = "fsn1"
  ssh_keys    = [data.hcloud_ssh_key.primary-ssh-key.name]

  network {
    network_id = hcloud_network.network.id
    ip         = "10.0.1.2"
  }

  labels = {
    purpose = "nomad_node"
  }

  depends_on = [
    hcloud_network_subnet.nodes-subnet
  ]
}

resource "hcloud_server" "node3" {
  name        = "node3"
  server_type = "cx21"
  image       = "ubuntu-22.04"
  location    = "nbg1"
  ssh_keys    = [data.hcloud_ssh_key.primary-ssh-key.name]

  network {
    network_id = hcloud_network.network.id
    ip         = "10.0.1.3"
  }

  labels = {
    purpose = "nomad_node"
  }

  depends_on = [
    hcloud_network_subnet.nodes-subnet
  ]
}
