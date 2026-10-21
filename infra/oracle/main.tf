terraform {
  required_version = ">= 1.6, < 2.0"
  required_providers {
    oci = {
      source  = "oracle/oci"
      version = "~> 7.25.0"
    }
  }
}

provider "oci" {
  region              = var.home_region
  config_file_profile = var.oci_profile
}

# One Always Free eligible ARM VM. Account-wide quota must be checked before apply.
resource "oci_core_vcn" "game" {
  compartment_id = var.compartment_ocid
  cidr_block     = "10.42.0.0/16"
  display_name   = "jesus-sheep"
  dns_label      = "sheep"
}

resource "oci_core_internet_gateway" "game" {
  compartment_id = var.compartment_ocid
  vcn_id         = oci_core_vcn.game.id
  enabled        = true
}

resource "oci_core_route_table" "game" {
  compartment_id = var.compartment_ocid
  vcn_id         = oci_core_vcn.game.id
  route_rules {
    destination       = "0.0.0.0/0"
    destination_type  = "CIDR_BLOCK"
    network_entity_id = oci_core_internet_gateway.game.id
  }
}

resource "oci_core_security_list" "game" {
  compartment_id = var.compartment_ocid
  vcn_id         = oci_core_vcn.game.id
  egress_security_rules {
    protocol    = "all"
    destination = "0.0.0.0/0"
  }
  ingress_security_rules {
    protocol = "6"
    source   = var.ssh_source_cidr
    tcp_options {
      min = 22
      max = 22
    }
  }
  dynamic "ingress_security_rules" {
    for_each = [80, 443]
    content {
      protocol = "6"
      source   = "0.0.0.0/0"
      tcp_options {
        min = ingress_security_rules.value
        max = ingress_security_rules.value
      }
    }
  }
  ingress_security_rules {
    protocol = "1"
    source   = "0.0.0.0/0"
    icmp_options {
      type = 3
      code = 4
    }
  }
}

resource "oci_core_subnet" "game" {
  compartment_id    = var.compartment_ocid
  vcn_id            = oci_core_vcn.game.id
  cidr_block        = "10.42.1.0/24"
  route_table_id    = oci_core_route_table.game.id
  security_list_ids = [oci_core_security_list.game.id]
  dns_label         = "api"
}

resource "oci_core_instance" "api" {
  compartment_id      = var.compartment_ocid
  availability_domain = var.availability_domain
  display_name        = "jesus-sheep-api"
  shape               = "VM.Standard.A1.Flex"
  shape_config {
    ocpus         = 1
    memory_in_gbs = 6
  }
  source_details {
    source_type             = "image"
    source_id               = var.ubuntu_arm_image_ocid
    boot_volume_size_in_gbs = 50
    boot_volume_vpus_per_gb = 10
  }
  create_vnic_details {
    subnet_id        = oci_core_subnet.game.id
    assign_public_ip = true
  }
  metadata = {
    ssh_authorized_keys = trimspace(file(pathexpand(var.ssh_public_key_path)))
    user_data           = base64encode(file("${path.module}/cloud-init.yaml"))
  }
  preserve_boot_volume = true
  lifecycle {
    prevent_destroy = true
  }
}

output "public_ip" {
  value = oci_core_instance.api.public_ip
}
