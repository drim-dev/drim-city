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

resource "hcloud_firewall" "firewall-ingress-ssh" {
  name   = "firewall-ingress-ssh"

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
}

resource "hcloud_firewall" "firewall-ingress-frontend" {
  name   = "firewall-ingress-frontend"

  rule {
    destination_ips = []
    direction       = "in"
    port            = "80"
    protocol        = "tcp"
    source_ips = [
      "0.0.0.0/0",
      "::/0",
    ]
  }

  rule {
    destination_ips = []
    direction       = "in"
    port            = "80"
    protocol        = "udp"
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

  rule {
    destination_ips = []
    direction       = "in"
    port            = "443"
    protocol        = "udp"
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

  rule {
    direction       = "out"
    port            = "80"
    protocol        = "tcp"
    destination_ips = [
      "0.0.0.0/0",
      "::/0",
    ]
  }

  rule {
    direction       = "out"
    port            = "80"
    protocol        = "udp"
    destination_ips = [
      "0.0.0.0/0",
      "::/0",
    ]
  }

  rule {
    direction       = "out"
    port            = "443"
    protocol        = "tcp"
    destination_ips = [
      "0.0.0.0/0",
      "::/0",
    ]
  }

  rule {
    direction       = "out"
    port            = "443"
    protocol        = "udp"
    destination_ips = [
      "0.0.0.0/0",
      "::/0",
    ]
  }
}

resource "hcloud_server" "frontend" {
  name        = "frontend"
  server_type = "cx21"
  image       = "ubuntu-22.04"
  location    = "hel1"
  ssh_keys    = [data.hcloud_ssh_key.primary-ssh-key.name]

  network {
    network_id = hcloud_network.network.id
    ip         = "10.0.1.1"
  }

  firewall_ids = [
    hcloud_firewall.firewall-ingress-ssh.id,
    hcloud_firewall.firewall-egress.id,
    hcloud_firewall.firewall-ingress-frontend.id
  ]

  labels = {
    purpose = "frontend"
  }

  depends_on = [
    hcloud_network_subnet.nodes-subnet
  ]
}

resource "hcloud_server" "backend" {
  name        = "backend"
  server_type = "cx21"
  image       = "ubuntu-22.04"
  location    = "hel1"
  ssh_keys    = [data.hcloud_ssh_key.primary-ssh-key.name]

  network {
    network_id = hcloud_network.network.id
    ip         = "10.0.1.2"
  }

  firewall_ids = [
    hcloud_firewall.firewall-ingress-ssh.id,
    hcloud_firewall.firewall-egress.id
  ]

  labels = {
    purpose = "backend"
  }

  depends_on = [
    hcloud_network_subnet.nodes-subnet
  ]
}

resource "hcloud_server" "database" {
  name        = "database"
  server_type = "cx21"
  image       = "ubuntu-22.04"
  location    = "hel1"
  ssh_keys    = [data.hcloud_ssh_key.primary-ssh-key.name]

  network {
    network_id = hcloud_network.network.id
    ip         = "10.0.1.3"
  }

  firewall_ids = [
    hcloud_firewall.firewall-ingress-ssh.id,
    hcloud_firewall.firewall-egress.id
  ]

  labels = {
    purpose = "database"
  }

  depends_on = [
    hcloud_network_subnet.nodes-subnet
  ]
}

resource "hcloud_volume" "database" {
  name              = "database"
  size              = 10
  server_id         = hcloud_server.database.id
  automount         = true
  format            = "ext4"
  delete_protection = true

  labels = {
    purpose = "database"
  }
}
