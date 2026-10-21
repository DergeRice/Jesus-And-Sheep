variable "home_region" {
  description = "OCI tenancy home region; free compute/storage must be created here."
  type        = string
}
variable "oci_profile" {
  type    = string
  default = "DEFAULT"
}
variable "compartment_ocid" {
  type = string
}
variable "availability_domain" {
  description = "Full availability-domain name from OCI, including tenancy prefix."
  type        = string
}
variable "ubuntu_arm_image_ocid" {
  description = "Always Free eligible Canonical Ubuntu 22.04 ARM64 image OCID in the home region."
  type        = string
}
variable "ssh_public_key_path" {
  type = string
}
variable "ssh_source_cidr" {
  description = "Administrator's public IPv4 address, e.g. 203.0.113.10/32."
  type        = string
  validation {
    condition     = can(cidrhost(var.ssh_source_cidr, 0)) && endswith(var.ssh_source_cidr, "/32")
    error_message = "Use a single trusted IPv4 address with /32 for SSH."
  }
}
